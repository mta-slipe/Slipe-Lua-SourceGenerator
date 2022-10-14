using CSharpLua;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using SlipeLua.CodeGenerator.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SlipeLua.CodeGenerator
{
    public struct ManifestJson
    {
        public string[] Modules { get; set; }
        public string[] Types { get; set; }
        public string[] RequiredAssemblies { get; set; }
    }

    [Generator]
    public class LuaSourceGenerator : ISourceGenerator
    {
        private CSharpCompilation compilation;

        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                //System.Diagnostics.Debugger.Launch();

                var compilation = (CSharpCompilation)context.Compilation;
                if (!ShouldCompileToLua(compilation))
                    return;

                var input = GetLongestCommonPrefix(context.Compilation.SyntaxTrees.Select(x => Path.GetDirectoryName(x.FilePath)).ToArray());
                Directory.SetCurrentDirectory(input);

                this.compilation = compilation;
                var output = Path.Combine(input, $"Lua/Dist/{compilation.AssemblyName}");
                Directory.CreateDirectory(output);

                var isServer = DetermineIfIsServer(compilation);
                var type = 
                    !isServer.HasValue ? "shared" :
                    isServer.Value ? "server" : "client";

                var requiredAssemblies = CodeGenerationConstants.StandardAssembliesByType[type]
                    .Concat(DetermineAdditionalRequiredAssemblies(compilation));

                var libs = requiredAssemblies.Select(x => $"{x}!").Except(new string[] { compilation.AssemblyName });
                var compiler = new Compiler(input, output, string.Join(";", libs), null, null, true, "SlipeLua.Shared.Elements.DefaultElementClassAttribute", "")
                {
                    IsExportMetadata = true,
                    IsModule = !isServer.HasValue,
                };
                var syntaxGenerator = compiler.Compile(compilation);
                GenerateManifestJson(
                    output, 
                    syntaxGenerator.Modules, 
                    syntaxGenerator.ExportTypes, 
                    isServer.HasValue ? requiredAssemblies : new string[0]);

                    if (isServer.HasValue)
                    {
                        GenerateMetaXml("./", type, requiredAssemblies);
                        GenerateEntryPointFile("./", compilation.AssemblyName);
                        GenerateLuaFiles("./");
                    }

                }
            catch (System.Exception e)
            {
                if (Environment.GetEnvironmentVariable("SLIPE-LUA-DEBUG") == "ENABLED")
                    System.Diagnostics.Debugger.Launch();

                context.ReportDiagnostic(Diagnostic.Create("SLIPE-LUA-ERROR", "Errors", e.Message, DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 1));

                throw;
            }
        }

        public static string GetBaseFilepath(Compilation compilation)
        {
            return GetLongestCommonPrefix(compilation.SyntaxTrees.Select(x => Path.GetDirectoryName(x.FilePath)).ToArray());
        }

        private static string GetLongestCommonPrefix(string[] s)
        {
            int k = s[0].Length;
            for (int i = 1; i < s.Length; i++)
            {
                k = Math.Min(k, s[i].Length);
                for (int j = 0; j < k; j++)
                    if (s[i][j] != s[0][j])
                    {
                        k = j;
                        break;
                    }
            }
            return s[0].Substring(0, k);
        }

        private bool ShouldCompileToLua(CSharpCompilation compilation)
        {
            return compilation.SyntaxTrees.Any(x => 
                x.ContainsAttribute(new string[] { "CompileToLua", "ClientEntryPoint", "ServerEntryPoint", "RequiresAssembly" }) 
            );
        }

        private bool? DetermineIfIsServer(CSharpCompilation compilation)
        {
            var entryPoint = compilation.GetEntryPoint(new System.Threading.CancellationToken());
            if (entryPoint != null)
            {
                var method = entryPoint.GetDeclaringSyntaxNode();
                var attributes = method.ChildNodes()
                    .Where(x => x is AttributeListSyntax)
                    .Select(x => (AttributeListSyntax)x)
                    .SelectMany(x => x.ChildNodes());

                if (attributes == null)
                    return null;

                if (attributes.Any(x => (x as AttributeSyntax)?.Name.ToString() == "ServerEntryPoint"))
                    return true;

                if (attributes.Any(x => (x as AttributeSyntax)?.Name.ToString() == "ClientEntryPoint"))
                    return false;
            }
            return null;
        }

        private IEnumerable<string> DetermineAdditionalRequiredAssemblies(CSharpCompilation compilation)
        {
            var entryPoint = compilation.GetEntryPoint(new System.Threading.CancellationToken());
            if (entryPoint != null)
            {
                var method = entryPoint.GetDeclaringSyntaxNode();
                var attributes = method.ChildNodes()
                    .Where(x => x is AttributeListSyntax)
                    .Select(x => (AttributeListSyntax)x)
                    .SelectMany(x => x.ChildNodes());

                if (attributes == null)
                    return Enumerable.Empty<string>();

                return attributes
                    .Select(x => x as AttributeSyntax)
                    .Where(x => x != null)
                    .Where(x => x.Name.ToString() == "RequiresAssembly")
                    .Select(x => x.ArgumentList.Arguments.First())
                    .Select(x => x.Expression as LiteralExpressionSyntax)
                    .Select(x => x.Token.ValueText);
            }
            return Enumerable.Empty<string>();
        }

        private void GenerateManifestJson(
            string outputDirectory, 
            IEnumerable<string> modules, 
            IEnumerable<string> types,
            IEnumerable<string> requiredAssemblies
        )
        {
            var content = JsonConvert.SerializeObject(new ManifestJson()
            {
                Modules = modules.ToArray(),
                Types = types.ToArray(),
                RequiredAssemblies = requiredAssemblies.ToArray()
            });
            File.WriteAllText(Path.Combine(outputDirectory, "manifest.json"), content);
        }

        private void GenerateEntryPointFile(string outputDirectory, string assemblyName)
        {
            File.WriteAllText(Path.Combine(outputDirectory, "entrypoint.slipe"), assemblyName);
        }

        private void GenerateLuaFiles(string outputDirectory)
        {
            File.WriteAllText(Path.Combine(outputDirectory, "Lua/patches.lua"), CodeGenerationConstants.Patches);
            File.WriteAllText(Path.Combine(outputDirectory, "Lua/main.lua"), CreateMainFile());
        }

        private string CreateMainFile()
        {
            var assemblies = "";

            var entryPoint = compilation.GetEntryPoint(new System.Threading.CancellationToken());
            var entryPointClass = entryPoint.ContainingType.Name;
            var entryPointNamespace = entryPoint.ContainingNamespace.ToDisplayString();

            return CodeGenerationConstants.Main
                .Replace("__ASSEMBLIES__", assemblies)
                .Replace("__ENTRYPOINT__", $"{entryPointNamespace}.{entryPointClass}.{entryPoint.Name}()");
        }

        private void GenerateMetaXml(string outputDirectory, string type, IEnumerable<string> requiredAssemblies)
        {
            var meta = new XmlDocument();

            var root = meta.CreateElement("meta");
            meta.AppendChild(root);

            AddScript(meta, root, "Lua/patches.lua", type);
            AddScript(meta, root, "Lua/main.lua", type);

            meta.Save(Path.Combine(outputDirectory, "meta.xml"));
        }

        private void AddScript(XmlDocument document, XmlElement parent, string path, string type)
        {
            var element = document.CreateElement("script");
            element.SetAttribute("src", path);
            element.SetAttribute("type", type);
            parent.AppendChild(element);
        }
    }
}
