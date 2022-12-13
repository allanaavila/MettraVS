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

namespace SyntaxWalker
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            const string programText =
                                @"using System;
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Text;
                                using Microsoft.CodeAnalysis;
                                using Microsoft.CodeAnalysis.CSharp;

                                    namespace TopLevel
                                    {
                                        using Microsoft;
                                        using System.ComponentModel;

                                        namespace Child1
                                        {
                                            using Microsoft.Win32;
                                            using System.Runtime.InteropServices;

                                            class Foo { }
                                        }

                                        namespace Child2
                                        {
                                            using System.CodeDom;
                                            using Microsoft.CSharp;

                                        class Bar { }
                                        }
                                }";

            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            var collector = new UsingCollector();
            collector.Visit(root);
            foreach (var directive in collector.Usings)
            {
                Console.WriteLine(directive.Name);
            }

            // Tente definir a versão do MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // Se houver apenas uma instância do MSBuild nesta máquina, defina-a como aquela a ser usada.
                ? visualStudioInstances[0]
                // Manipule a seleção da versão do MSBuild que você deseja usar.
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Usando o MSBuild em '{instance.MSBuildPath}' para carregar projetos.");

            // NOTA: Certifique-se de registrar uma instância com o MSBuildLocator
            //       antes de chamar MSBuildWorkspace.Create()
            //       caso contrário, o MSBuildWorkspace não comporá MEF.
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Imprima a mensagem para o evento WorkspaceFailed para ajudar a diagnosticar falhas de carregamento do projeto.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                var solutionPath = args[0];
                Console.WriteLine($"Carregando solução '{solutionPath}' \n");

                // Anexe um relatório de progresso para que possamos imprimir os projetos à medida que são carregados.
                var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
                Console.WriteLine($"Solução de carregamento finalizada '{solutionPath}' \n");

                // TODO: Faça análises nos projetos na solução carregada
            }
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Várias instalações do MSBuild detectadas, selecione uma: ");
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
