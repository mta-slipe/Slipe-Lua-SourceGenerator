using CSharpLua;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace CodeGenerator
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
                var input = Directory.GetCurrentDirectory();
                var compilation = (CSharpCompilation)context.Compilation;
                this.compilation = compilation;
                var output = Path.Combine(input, $"Lua/Dist/{compilation.AssemblyName}");
                Directory.CreateDirectory(output);

                var isServer = DetermineIfIsServer(compilation);
                var type = 
                    !isServer.HasValue ? "shared" :
                    isServer.Value ? "server" : "client";

                var requiredAssemblies = CodeGenerationConstants.StandardAssembliesByType[type];

                //System.Diagnostics.Debugger.Launch();
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
                        GenerateLuaFiles("./", type, compilation.AssemblyName, requiredAssemblies);
                    }

                }
            catch (System.Exception e)
            {
                System.Diagnostics.Debugger.Launch();
                throw;
            }
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
                    .First();

                if (attributes.ChildNodes().Any(x => (x as AttributeSyntax)?.Name.ToString() == "ServerEntryPoint"))
                    return true;

                if (attributes.ChildNodes().Any(x => (x as AttributeSyntax)?.Name.ToString() == "ClientEntryPoint"))
                    return false;
            }
            return null;
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

        private void GenerateMetaXml(string outputDirectory, string type, IEnumerable<string> requiredAssemblies)
        {
            var meta = new XmlDocument();

            var root = meta.CreateElement("meta");
            meta.AppendChild(root);

            //foreach (var assembly in requiredAssemblies)
            //{
            //    var manifestLocation = Path.Combine(outputDirectory, "Lua/Dist", assembly, "manifest.json");
            //    var manifest = JsonConvert.DeserializeObject<ManifestJson>(File.ReadAllText(manifestLocation));
            //    var modules = manifest.Modules;
            //    foreach (var module in modules)
            //        AddScript(meta, root, Path.Combine(outputDirectory, "Lua/Dist", module), type);
            //}

            AddScript(meta, root, "Lua/patches.lua", type);
            AddScript(meta, root, "Lua/main.lua", type);

            meta.Save(Path.Combine(outputDirectory, "meta.xml"));
        }

        private void GenerateLuaFiles(string outputDirectory, string type, string thisAssembly, IEnumerable<string> requiredAssemblies)
        {
            File.WriteAllText(Path.Combine(outputDirectory, "Lua/patches.lua"), CodeGenerationConstants.Patches);
            File.WriteAllText(Path.Combine(outputDirectory, "Lua/main.lua"), CreateMainFile(type, thisAssembly, requiredAssemblies));
        }

        private string CreateMainFile(string type, string thisAssembly, IEnumerable<string> requiredAssemblies)
        {
            var assemblies = "";
            var additionalAssemblies = requiredAssemblies;
            foreach (var assembly in additionalAssemblies)
                assemblies += $"{assembly}Manifest()\n";

            assemblies += $"{thisAssembly}Manifest()\n";

            var entryPoint = compilation.GetEntryPoint(new System.Threading.CancellationToken());
            var entryPointClass = entryPoint.ContainingType.Name;
            var entryPointNamespace = entryPoint.ContainingNamespace.Name;
            //System.Diagnostics.Debugger.Launch();
            return CodeGenerationConstants.Main
                .Replace("__ASSEMBLIES__", assemblies)
                .Replace("__ENTRYPOINT__", $"{entryPointNamespace}.{entryPointClass}.{entryPoint.Name}()");
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
