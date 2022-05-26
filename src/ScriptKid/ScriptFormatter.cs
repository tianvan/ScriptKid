using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ScriptKid;

internal class ScriptFormatter : IScriptFormatter
{
    public string Format(string originalScript)
    {
        SyntaxTree? synaxTree = CSharpSyntaxTree.ParseText(originalScript);

        IEnumerable<Diagnostic>? errors = synaxTree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);
        if (errors.Any()) throw new ArgumentException($"Invalid script, {string.Join(',', errors.Select(x => x.GetMessage()))}", nameof(originalScript));

        CompilationUnitSyntax? root = synaxTree.GetCompilationUnitRoot();
        return root.Members.ToString();
    }
}
