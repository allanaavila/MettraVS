using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeManualTraversal
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            const string programText = @"using System;
                                        using System.Collections;
                                        using System.Linq;
                                        using System.Text;
                                        using SyntaxTreeManualTraversal.Contas;
                                        using SyntaxTreeManualTraversal.Titular;

                                        namespace Bytebank
                                        {
                                            class Program
                                             {
                                                 static void Main(string[] args)
                                                    {
                                                        conta.Titular = new Cliente();
                                                        Console.WriteLine(conta.Numero_agencia);
                                                        Console.WriteLine(conta.Conta);
                                                        Console.WriteLine(conta.titular);
                                                        Console.WriteLine(conta.GetSaldo());
                                                    }
                                             }
                                        }";

            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            Console.WriteLine($"A árvore é um {root.Kind()} nó. \n");
            Console.WriteLine($"A árvore tem {root.Members.Count} elementos nela. \n");
            Console.WriteLine($"A árvore tem {root.Usings.Count} usando instruções. Eles são: \n");
            foreach (UsingDirectiveSyntax element in root.Usings)
                Console.WriteLine($"\t{element.Name}");

            MemberDeclarationSyntax firstMember = root.Members[0];
            Console.WriteLine($"O primeiro membro é um {firstMember.Kind()}. \n");
            var helloWorldDeclaration = (NamespaceDeclarationSyntax)firstMember;

            Console.WriteLine($"Existem  {helloWorldDeclaration.Members.Count} membros declarados neste namespace. \n");
            Console.WriteLine($"O primeiro membro é um {helloWorldDeclaration.Members[0].Kind()}. \n");

            var programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];
            Console.WriteLine($"Há {programDeclaration.Members.Count} membros declarados na {programDeclaration.Identifier} classe. \n");
            Console.WriteLine($"O primeiro membro é {programDeclaration.Members[0].Kind()}. \n");
            var mainDeclaration = (MethodDeclarationSyntax)programDeclaration.Members[0];

            Console.WriteLine($"O tipo do retorno do {mainDeclaration.Identifier} método é {mainDeclaration.ReturnType}. \n");
            Console.WriteLine($"O método tem {mainDeclaration.ParameterList.Parameters.Count} parâmetros. \n");
            foreach (ParameterSyntax item in mainDeclaration.ParameterList.Parameters)
                Console.WriteLine($"O tipo do {item.Identifier} parâmetro é {item.Type}. \n");
            Console.WriteLine($"O texto do corpo do {mainDeclaration.Identifier} método segue: \n");
            Console.WriteLine(mainDeclaration.Body.ToFullString());

            var argsParameter = mainDeclaration.ParameterList.Parameters[0];

            var firstParameters = from methodDeclaration in root.DescendantNodes()
                                        .OfType<MethodDeclarationSyntax>()
                                  where methodDeclaration.Identifier.ValueText == "Main"
                                  select methodDeclaration.ParameterList.Parameters.First();

            var argsParameter2 = firstParameters.Single();

            Console.WriteLine(argsParameter == argsParameter2);

            // Tente definir a versão do MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // Se houver apenas uma instância do MSBuild nesta máquina, defina-a como aquela a ser usada.
                ? visualStudioInstances[0]
                // Manipule a seleção da versão do MSBuild que você deseja usar.
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Usando o MSBuild em '{instance.MSBuildPath}' para carregar projetos.");

            // NOTE: Certifique-se de registrar uma instância com o MSBuildLocator
            //       antes de chamar MSBuildWorkspace.Create()
            //       caso contrário, o MSBuildWorkspace não comporá MEF.
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Imprima a mensagem para o evento WorkspaceFailed para ajudar a diagnosticar falhas de carregamento do projeto.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                var solutionPath = args[0];
                Console.WriteLine($"Carregando solução '{solutionPath}'");

                // Anexe um relatório de progresso para que possamos imprimir os projetos à medida que são carregados.
                var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
                Console.WriteLine($"Solução de carregamento finalizada '{solutionPath}'");

                // TODO: Faça análises nos projetos na solução carregada
            }
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Várias instalações do MSBuild detectadas, selecione uma:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instância {i + 1}");
                Console.WriteLine($"    Nome: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Versão: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    Caminho do MSBuild: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Entrada não aceita, tente novamente.");
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}
