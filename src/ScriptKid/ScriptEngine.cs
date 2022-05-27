using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;

using ScriptKid;

namespace Microsoft.Extensions.DependencyInjection;

public class ScriptEngine : IScriptEngine
{
    private readonly IScriptFormatter _scriptFormatter;
    private readonly IScriptDigestComputer _scriptDigestComputer;
    private readonly ICompilationInfoStorage _compilationInfoStorage;
    private readonly ILogger<ScriptEngine> _logger;

    public ScriptEngine(IScriptFormatter scriptFormatter, IScriptDigestComputer scriptDigestComputer, ICompilationInfoStorage compilationInfoStorage, ILogger<ScriptEngine> logger)
    {
        _scriptFormatter = scriptFormatter;
        _scriptDigestComputer = scriptDigestComputer;
        _compilationInfoStorage = compilationInfoStorage;
        _logger = logger;
    }

    public async Task<TResult?> RunAsync<TResult>(string script, object? globals = default, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(script)) throw new ArgumentException($"“{nameof(script)}”不能为 null 或空白。", nameof(script));
        if (globals is not null)
        {
            if (globals.GetType().GetCustomAttribute<CompilerGeneratedAttribute>() is not null)
            {
                throw new ArgumentException($"“{nameof(globals)}”不能是编译器生成的类型。", nameof(globals));
            }
        }

        var formattedScript = _scriptFormatter.Format(script);
        var digest = _scriptDigestComputer.ComputeDigest(formattedScript, globals);

        var has = await _compilationInfoStorage.TryGetAsync(digest, out Stream? assemblyStream);
        if (has)
        {
            return await RunAsyncCore<TResult>(assemblyStream!, globals, cancellationToken);
        }

        (MemoryStream dllStream, MemoryStream pdbStream) = Compile<TResult>(formattedScript, globals);
        var compilationInfo = new CompilationInfo(digest, dllStream.ToArray(), pdbStream.ToArray());
        var saved = await _compilationInfoStorage.TrySaveAsync(compilationInfo);
        pdbStream.Dispose();

        if (!saved)
        {
            _logger.LogWarning("Save CompilationInfo failed. the {compilation} is existed.", digest);
            throw new InvalidOperationException("Save CompilationInfo failed.");
        }

        return await RunAsyncCore<TResult>(dllStream, globals, cancellationToken);
    }

    private (MemoryStream dllStream, MemoryStream pdbStream) Compile<TResult>(string formattedScript, object? globals)
    {
        ScriptOptions? scriptOptions = ScriptOptions.Default
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithReferences(typeof(Task).GetTypeInfo().Assembly, typeof(TResult).GetTypeInfo().Assembly)
            .WithImports("System", "System.Threading.Tasks");

        Script<TResult> script = globals is null
            ? CSharpScript.Create<TResult>(formattedScript, scriptOptions)
            : CSharpScript.Create<TResult>(formattedScript, scriptOptions, globals!.GetType());

        Compilation compilation = script.GetCompilation();
        var dllStream = new MemoryStream();
        var pdbStream = new MemoryStream();
        compilation.Emit(dllStream, pdbStream);
        dllStream.Position = 0;
        pdbStream.Position = 0;
        return (dllStream, pdbStream);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<TResult?> RunAsyncCore<TResult>(Stream assemblyStream, object? globals, CancellationToken cancellationToken)
    {
        if (assemblyStream is null) throw new ArgumentNullException(nameof(assemblyStream));

        var context = new CollectibleAssemblyLoadContext();
        Assembly assembly = context.LoadFromStream(assemblyStream);
        assemblyStream.Dispose();

        MethodInfo entryPoint = GetEntryPoint(assembly);

        var parameters = new object[]
        {
            new object?[] { globals, null }
        };
        TResult? result = await (entryPoint.Invoke(null, parameters) as Task<TResult?>)!
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);

        context.Unload();
        return result;
    }

    public MethodInfo GetEntryPoint(Assembly assembly)
    {
        Type entryType = assembly.GetExportedTypes().AsParallel().Single(t => t.Name.Contains("Submission#"));
        return entryType.GetMethod("<Factory>")!;
    }
}