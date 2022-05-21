using System.Security.Cryptography;

namespace ScriptKid;

public class ScriptDigestComputer : IScriptDigestComputer
{
    public string ComputeDigest(string formattedScript)
    {
        if (string.IsNullOrWhiteSpace(formattedScript)) throw new ArgumentException($"“{nameof(formattedScript)}”不能为 null 或空白。", nameof(formattedScript));

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(formattedScript));
        return Convert.ToBase64String(bytes);
    }
}
