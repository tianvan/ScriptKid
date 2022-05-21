namespace ScriptKid;

public class CompilationInfo
{
    public CompilationInfo(string scriptDigest, byte[] assemblyBinary)
    {
        if (string.IsNullOrWhiteSpace(scriptDigest)) throw new ArgumentException($"“{nameof(scriptDigest)}”不能为 null 或空白。", nameof(scriptDigest));
        ScriptDigest = scriptDigest;
        AssemblyBinary = assemblyBinary ?? throw new ArgumentNullException(nameof(assemblyBinary));
    }

    public string ScriptDigest { get; protected set; } = default!;

    public byte[] AssemblyBinary { get; protected set; } = default!;
}
