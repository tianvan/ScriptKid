namespace ScriptKid;

public interface ICompilationInfoStorage
{
    ValueTask<bool> TrySaveAsync(CompilationInfo compilationInfo);

    ValueTask<bool> TryGetAsync(string scriptDigest, out Stream? assemblyStream);
}
