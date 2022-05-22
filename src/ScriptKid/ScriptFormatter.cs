namespace ScriptKid;

internal class ScriptFormatter : IScriptFormatter
{
    public string Format(string originalScript)
    {
        if (string.IsNullOrWhiteSpace(originalScript)) throw new ArgumentException($"“{nameof(originalScript)}”不能为 null 或空白。", nameof(originalScript));

        return originalScript;
    }
}
