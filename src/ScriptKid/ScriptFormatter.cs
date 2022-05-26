using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ScriptKid;

internal class ScriptFormatter : IScriptFormatter
{
    public string Format(string originalScript)
    {
        SyntaxTree? synaxTree = CSharpSyntaxTree.ParseText(originalScript);
        CompilationUnitSyntax? root = synaxTree.GetCompilationUnitRoot();
        return root.ToString();
    }
}
