using System.Collections.Generic;

namespace SlipeLua.CodeGenerator;

public class CodeGenerationConstants
{
    public const string Main = """
        function main()
            postPatches()
            __ASSEMBLIES__
            initEvents()
            __ENTRYPOINT__
        end

        addEventHandler("onResourceStart", resourceRoot, main)
        addEventHandler("onClientResourceStart", resourceRoot, main)
        """;

    public static string Patches => CodeGenerationPatches.All;

    public static Dictionary<string, string[]> StandardAssembliesByType { get; } = new Dictionary<string, string[]>()
    {
        ["shared"] = ["SlipeLua.MTADefinitions", "SlipeLua.Shared", "SlipeLua.Server", "SlipeLua.Sql", "SlipeLua.Client"],
        ["server"] = ["SlipeLua.MTADefinitions", "SlipeLua.Shared", "SlipeLua.Server", "SlipeLua.Sql"],
        ["client"] = ["SlipeLua.MTADefinitions", "SlipeLua.Shared", "SlipeLua.Client"]
    };
}
