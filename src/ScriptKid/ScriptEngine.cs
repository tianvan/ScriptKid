using System.Reflection;
using System.Runtime.Loader;

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

    public async Task<TResult> RunAsync<TResult>(string script, object? globals = default)
    {
        if (string.IsNullOrWhiteSpace(script)) throw new ArgumentException($"“{nameof(script)}”不能为 null 或空白。", nameof(script));

        var formattedScript = _scriptFormatter.Format(script);
        var digest = _scriptDigestComputer.ComputeDigest(formattedScript);

        var has = await _compilationInfoStorage.TryGetAsync(digest, out Stream? assemblyStream);
        if (has)
        {
            return await RunAsyncCore<TResult>(assemblyStream!, globals);
        }

        MemoryStream compilationSteam = Compile<TResult>(formattedScript, globals);

        var compilationInfo = new CompilationInfo(digest, compilationSteam.ToArray());
        var saved = await _compilationInfoStorage.TrySaveAsync(compilationInfo);
        if (!saved)
        {
            _logger.LogWarning("Save CompilationInfo failed. the {compilation} is existed.", digest);
            throw new InvalidOperationException("Save CompilationInfo failed.");
        }

        return await RunAsyncCore<TResult>(assemblyStream!, globals);
    }

    private MemoryStream Compile<TResult>(string formattedScript, object? globals)
    {
        Script<TResult> script = globals is null
            ? CSharpScript.Create<TResult>(formattedScript, ScriptOptions.Default)
            : CSharpScript.Create<TResult>(formattedScript, ScriptOptions.Default, globals!.GetType());

        Compilation compilation = script.GetCompilation();
        var stream = new MemoryStream();
        compilation.Emit(stream);
        stream.Position = 0;
        return stream;
    }

    private Task<TResult> RunAsyncCore<TResult>(Stream AssemblyStream, object? globals)
    {
        if (AssemblyStream is null) throw new ArgumentNullException(nameof(AssemblyStream));

        var context = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
        Assembly assembly = context.LoadFromStream(AssemblyStream);
        MethodInfo entryPoint = GetEntryPoint(assembly);

        var parameters = new object[]
        {
            new object?[] { globals, null }
        };
        var task = entryPoint.Invoke(null, parameters) as Task<TResult>;

        return task!;
    }

    public MethodInfo GetEntryPoint(Assembly assembly)
    {
        Type entryType = assembly.GetExportedTypes().AsParallel().Single(t => t.Name.Contains("Submission#"));
        return entryType.GetMethod("<Factory>")!;
    }
}