using static System.Console;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace SyntaxWalker
{
    class UsingCollector : CSharpSyntaxWalker
    {

        public ICollection<UsingDirectiveSyntax> Usings { get; } = new List<UsingDirectiveSyntax>();

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            WriteLine($"\tVisitUsingDirective chamado com {node.Name}.");
            if (node.Name.ToString() != "Sistema" &&
                !node.Name.ToString().StartsWith("Sistema."))
            {
                WriteLine($"\t\tSucesso. Adicionando {node.Name}.");
                this.Usings.Add(node);
            }
        }

    }
}
