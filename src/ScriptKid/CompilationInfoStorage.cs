using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ScriptKid;

public class CompilationInfoStorage : ICompilationInfoStorage
{
    private readonly ScriptEngineOptions _scriptEngineOptions;
    private readonly ILogger<CompilationInfoStorage> _logger;

    public CompilationInfoStorage(IOptions<ScriptEngineOptions> scriptEngineOptionsAccessor, ILogger<CompilationInfoStorage> logger)
    {
        _scriptEngineOptions = scriptEngineOptionsAccessor.Value;
        _logger = logger;
    }

    public ValueTask<bool> TryGetAsync(string scriptDigest, out Stream? assemblyStream)
    {
        EnsureScriptsFolderExists();

        var path = Path.Combine(_scriptEngineOptions.ScriptsPath, scriptDigest + ".Dll");
        if (!File.Exists(path))
        {
            assemblyStream = null;

            _logger.LogDebug($"Script not found: {path}");

            return new ValueTask<bool>(false);
        }

        assemblyStream = File.OpenRead(path);
        return new ValueTask<bool>(true);
    }

    private void EnsureScriptsFolderExists()
    {
        Directory.CreateDirectory(_scriptEngineOptions.ScriptsPath);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public ValueTask<bool> TrySaveAsync(CompilationInfo compilationInfo)
    {
        EnsureScriptsFolderExists();

        var dllPath = Path.Combine(_scriptEngineOptions.ScriptsPath, compilationInfo.ScriptDigest + ".Dll");
        if (File.Exists(dllPath))
        {
            _logger.LogWarning($"Script already exists: {dllPath}");

            return new ValueTask<bool>(false);
        }

        using var dllFileStream = new FileStream(dllPath, FileMode.Create);
        dllFileStream.Write(compilationInfo.Dll);

        var pdbPath = Path.Combine(_scriptEngineOptions.ScriptsPath, compilationInfo.ScriptDigest + ".Pdb");
        using var pdbFileStream = new FileStream(pdbPath, FileMode.Create);

        return new ValueTask<bool>(true);
    }
}
