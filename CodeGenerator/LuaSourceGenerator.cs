using CSharpLua;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SlipeLua.CodeGenerator.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;

namespace SlipeLua.CodeGenerator;

public struct ManifestJson
{
    public string[] Modules { get; set; }
    public string[] Types { get; set; }
    public string[] RequiredAssemblies { get; set; }
}

[Generator]
public class LuaSourceGenerator : ISourceGenerator
{
    private CSharpCompilation? compilation;

    public void Initialize(GeneratorInitializationContext context)
    {

    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();

            var compilation = (CSharpCompilation)context.Compilation;
            if (!ShouldCompileToLua(compilation))
                return;

            if (compilation.AssemblyName == null)
                throw new Exception("Assembly name is null, cannot compile to Lua.");

            var input = GetLongestCommonPrefix([.. context.Compilation.SyntaxTrees.Select(x => Path.GetDirectoryName(x.FilePath))]);
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

            var libs = requiredAssemblies.Select(x => $"{x}!")
                .Except([compilation.AssemblyName]);

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
                isServer.HasValue ? requiredAssemblies : []);

            if (isServer.HasValue)
            {
                GenerateFullPackage("./", type, requiredAssemblies);
                GenerateEntryPointFile("./", compilation.AssemblyName);
                GenerateLuaFiles("./");
            }

        }
        catch (System.Exception e)
        {
            //if (Environment.GetEnvironmentVariable("SLIPE-LUA-DEBUG") == "ENABLED")
            //    System.Diagnostics.Debugger.Launch();

            context.ReportDiagnostic(Diagnostic.Create("SLIPE01", "Errors", e.Message, DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 1));

            throw;
        }
    }

    public static string GetBaseFilepath(Compilation compilation)
    {
        return GetLongestCommonPrefix([.. compilation.SyntaxTrees.Select(x => Path.GetDirectoryName(x.FilePath))]);
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
            x.ContainsAttribute(["CompileToLua", "ClientEntryPoint", "ServerEntryPoint", "RequiresAssembly"])
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
                return [];

            return attributes
                .Select(x => x as AttributeSyntax)
                .Where(x => x != null)
                .Cast<AttributeSyntax>()
                .Where(x => x.Name.ToString() == "RequiresAssembly")
                .Select(x => x.ArgumentList?.Arguments.First())
                .Select(x => x?.Expression as LiteralExpressionSyntax)
                .Select(x => x?.Token.ValueText)
                .Where(x => x is not null)
                .Cast<string>();
        }
        return [];
    }

    private void GenerateManifestJson(
        string outputDirectory, 
        IEnumerable<string> modules, 
        IEnumerable<string> types,
        IEnumerable<string> requiredAssemblies
    )
    {
        var content = System.Text.Json.JsonSerializer.Serialize(new ManifestJson()
        {
            Modules = [.. modules],
            Types = [.. types],
            RequiredAssemblies = [.. requiredAssemblies]
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
        if (compilation == null)
            throw new Exception("Compilation is null, cannot create main.lua file.");

        var assemblies = "";

        var entryPoint = compilation.GetEntryPoint(new System.Threading.CancellationToken())
            ?? throw new Exception("No entry point found, cannot create main.lua file.");

        var entryPointClass = entryPoint.ContainingType.Name;
        var entryPointNamespace = entryPoint.ContainingNamespace.ToDisplayString();

        return CodeGenerationConstants.Main
            .Replace("__ASSEMBLIES__", assemblies)
            .Replace("__ENTRYPOINT__", $"{entryPointNamespace}.{entryPointClass}.{entryPoint.Name}()");
    }

    private void GenerateFullPackage(string outputDirectory, string type, IEnumerable<string> requiredAssemblies)
    {
        var meta = new XmlDocument();

        var root = meta.CreateElement("meta");
        meta.AppendChild(root);

        AddScript(meta, root, "Lua/patches.lua", type);
        AddScript(meta, root, "Lua/main.lua", type);

        var luaFilesPerAssembly = new Dictionary<string, List<string>>();
        var luaContainingAssemblies = new List<AssemblyInfo>();

        foreach (var reference in compilation.References)
        {
            var assemblyPath = reference.Display;
            if (string.IsNullOrEmpty(assemblyPath) || !File.Exists(assemblyPath))
                continue;

            var assemblyName = Path.GetFileName(assemblyPath).TrimEnd(".dll");

            using var stream = File.OpenRead(assemblyPath);
            using var peReader = new PEReader(stream);

            if (peReader.HasMetadata)
            {
                var mdReader = peReader.GetMetadataReader();
                foreach (var handle in mdReader.ManifestResources)
                {
                    var resource = mdReader.GetManifestResource(handle);
                    var name = mdReader.GetString(resource.Name);
                    if (name.EndsWith(".lua"))
                    {
                        var luaIndex = name.IndexOf(".lua.", StringComparison.OrdinalIgnoreCase);
                        var basePath = name.Substring(0, luaIndex);
                        var scriptPath = name
                            .Replace(basePath, "")
                            .Replace(".", "/")
                            .Replace("//", "/")
                            .Replace("Lua/Dist/", $"Lua/Dist/{assemblyName}/");

                        scriptPath = Regex.Replace(scriptPath, "/lua$", ".lua");
                        scriptPath = Regex.Replace(scriptPath, "^/", "");

                        if (!luaFilesPerAssembly.ContainsKey(assemblyName))
                            luaFilesPerAssembly[assemblyName] = [];

                        luaFilesPerAssembly[assemblyName].Add(scriptPath);
                    }
                }
            }

            if (luaFilesPerAssembly.ContainsKey(assemblyName))
            {
                var assemblyInfo = new AssemblyInfo(assemblyName, assemblyPath, luaFilesPerAssembly[assemblyName]);

                assemblyInfo.LuaFiles = assemblyInfo.LuaFiles
                    .OrderBy(x =>
                    {
                        if (MetaGenerationKnownFilePriority.PreUnknown.Contains(x))
                            return MetaGenerationKnownFilePriority.PreUnknown.IndexOf(x) - 1000;

                        if (MetaGenerationKnownFilePriority.PostUnknown.Contains(x))
                            return MetaGenerationKnownFilePriority.PostUnknown.IndexOf(x) + 1000;


                        if (x.EndsWith("manifest.lua"))
                            return 10000;

                        return 0;
                    })
                    .ToList();

                luaContainingAssemblies.Add(assemblyInfo);
            }
        }

        var dependenciesPerAssembly = new Dictionary<AssemblyInfo, List<AssemblyInfo>>();
        foreach (var assembly in luaContainingAssemblies)
        {
            var dependencies = GetDependencies(assembly.Path)
                .Where(x => luaContainingAssemblies.Any(y => y.Name == x.Name))
                .ToList();

            dependenciesPerAssembly[assembly] = dependencies;
        }

        var resolvedAssemblies = new HashSet<string>();
        while (luaContainingAssemblies.Any())
        {
            var nonDependantAssembly = luaContainingAssemblies
                .FirstOrDefault(x => dependenciesPerAssembly[x].All(y => resolvedAssemblies.Contains(y.Name)));

            if (nonDependantAssembly != null)
            {
                foreach (var file in nonDependantAssembly.LuaFiles)
                    AddScript(meta, root, file, type);

                resolvedAssemblies.Add(nonDependantAssembly.Name);
                luaContainingAssemblies.Remove(nonDependantAssembly);
            }
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        var luaFiles = Directory.GetFiles(Path.Combine(currentDirectory, "Lua"), "*.lua", SearchOption.AllDirectories);
        foreach (var luaFile in luaFiles)
        {
            var relativePath = luaFile.Replace(currentDirectory + Path.DirectorySeparatorChar, "").Replace("\\", "/");
            if (relativePath.EndsWith("Lua/patches.lua") || relativePath.EndsWith("Lua/main.lua"))
                continue;

            AddScript(meta, root, relativePath, type);
        }

        meta.Save(Path.Combine(outputDirectory, "meta.xml"));
    }

    private IEnumerable<AssemblyInfo> GetDependencies(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(stream);

        if (peReader.HasMetadata)
        {
            var mdReader = peReader.GetMetadataReader();
            var dependencies = mdReader.AssemblyReferences;

            foreach (var handle in dependencies)
            {
                var aref = mdReader.GetAssemblyReference(handle);
                var name = aref.GetAssemblyName();
                yield return new AssemblyInfo(name.Name, name.FullName);
            }
        }
    }

    private void AddScript(XmlDocument document, XmlElement parent, string path, string type)
    {
        var element = document.CreateElement("script");
        element.SetAttribute("src", path);
        element.SetAttribute("type", type);
        parent.AppendChild(element);
    }

    private class AssemblyInfo(string name, string path, IEnumerable<string>? luaFiles = null)
    {
        public string Name { get; } = name;
        public string Path { get; } = path;
        public List<string> LuaFiles { get; set; } = new(luaFiles ?? []);
    }
}
