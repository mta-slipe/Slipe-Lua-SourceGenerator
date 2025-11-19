using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace SlipeLua.CodeGenerator.Extensions;

public static class SyntaxTreeExtensions
{
    public static bool ContainsAttribute(this SyntaxTree tree, string attribute)
    {
        return ContainsAttribute(tree, [attribute]);
    }

    public static bool ContainsAttribute(this SyntaxTree tree, IEnumerable<string> attributes)
    {
        var attributeSet = new HashSet<string>(attributes);

        var root = tree.GetRoot(new System.Threading.CancellationToken());
        return ContainsAttribute(root, attributeSet);
    }

    private static bool ContainsAttribute(SyntaxNode node, ISet<string> attributes)
    {
        foreach (var child in node.ChildNodes())
        {
            if (child is AttributeListSyntax attributeList && ContainsAttribute(attributeList, attributes))
                return true;
            else if (ContainsAttribute(child, attributes))
                return true;
        }

        return false;
    }

    private static bool ContainsAttribute(AttributeListSyntax attributeList, ISet<string> attributes)
    {
        return attributeList.ChildNodes().Any(x => attributes.Contains((x as AttributeSyntax)?.Name?.ToString() ?? "unknown"));
    }
}
