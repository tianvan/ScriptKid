namespace ScriptKid;

public class CompilationInfo
{
    public CompilationInfo(string scriptDigest, byte[] dll, byte[] pdb)
    {
        if (string.IsNullOrWhiteSpace(scriptDigest)) throw new ArgumentException($"“{nameof(scriptDigest)}”不能为 null 或空白。", nameof(scriptDigest));

        ScriptDigest = scriptDigest;
        Dll = dll ?? throw new ArgumentNullException(nameof(dll));
        Pdb = pdb ?? throw new ArgumentNullException(nameof(pdb));
    }

    public string ScriptDigest { get; protected set; } = default!;

    public byte[] Dll { get; protected set; } = default!;

    public byte[] Pdb { get; protected set; } = default!;
}
