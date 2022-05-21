﻿
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
        var path = Path.Combine(_scriptEngineOptions.ScriptsPath, scriptDigest);
        if (!File.Exists(path))
        {
            assemblyStream = null;

            _logger.LogWarning($"Script not found: {path}");

            return new ValueTask<bool>(false);
        }

        assemblyStream = File.OpenRead(path);
        return new ValueTask<bool>(true);
    }

    public ValueTask<bool> TrySaveAsync(CompilationInfo compilationInfo)
    {
        var path = Path.Combine(_scriptEngineOptions.ScriptsPath, compilationInfo.ScriptDigest);
        if (File.Exists(path))
        {
            _logger.LogWarning($"Script already exists: {path}");

            return new ValueTask<bool>(false);
        }

        using var fileStream = new FileStream(path, FileMode.Create);
        fileStream.Write(compilationInfo.AssemblyBinary);
        return new ValueTask<bool>(true);
    }
}
