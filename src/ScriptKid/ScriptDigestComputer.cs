using System.Security.Cryptography;

namespace ScriptKid;

public class ScriptDigestComputer : IScriptDigestComputer
{
    public string ComputeDigest(string formattedScript, object? globals = default)
    {
        if (string.IsNullOrWhiteSpace(formattedScript)) throw new ArgumentException($"“{nameof(formattedScript)}”不能为 null 或空白。", nameof(formattedScript));

        var original = formattedScript;
        if (globals is not null) original = globals.GetType().GUID + formattedScript;

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(original));
        return Convert.ToHexString(bytes);
    }
}
