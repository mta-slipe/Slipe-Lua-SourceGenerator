using System.Collections.Generic;

namespace CodeGenerator
{
    public class CodeGenerationConstants
    {
        public const string Main = @"

prePatches()
require(""CoreSystem.Lua/All.lua"")(""CoreSystem.Lua"")
postPatches()
__ASSEMBLIES__
initEvents()
__ENTRYPOINT__

";

        public static string Patches => CodeGenerationPatches.All;

        public static Dictionary<string, string[]> StandardAssembliesByType { get; } = new Dictionary<string, string[]>()
        {
            ["shared"] = new string[] { "SlipeMTADefinitions", "SlipeShared", "SlipeServer", "SlipeSql", "SlipeClient" },
            ["server"] = new string[] { "SlipeMTADefinitions", "SlipeShared", "SlipeServer", "SlipeSql" },
            ["client"] = new string[] { "SlipeMTADefinitions", "SlipeShared", "SlipeClient" }
        };
    }
}
