namespace ScriptKid;

public interface IScriptDigestComputer
{
    string ComputeDigest(string formattedScript, object? globals = default);
}
