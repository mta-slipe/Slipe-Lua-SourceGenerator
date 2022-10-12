using System.Collections.Generic;

namespace SlipeLua.CodeGenerator
{
    public class CodeGenerationConstants
    {
        public const string Main = @"
postPatches()
__ASSEMBLIES__
initEvents()
__ENTRYPOINT__

";

        public static string Patches => CodeGenerationPatches.All;

        public static Dictionary<string, string[]> StandardAssembliesByType { get; } = new Dictionary<string, string[]>()
        {
            ["shared"] = new string[] { "SlipeLua.MTADefinitions", "SlipeLua.Shared", "SlipeLua.Server", "SlipeLua.Sql", "SlipeLua.Client" },
            ["server"] = new string[] { "SlipeLua.MTADefinitions", "SlipeLua.Shared", "SlipeLua.Server", "SlipeLua.Sql" },
            ["client"] = new string[] { "SlipeLua.MTADefinitions", "SlipeLua.Shared", "SlipeLua.Client" }
        };
    }
}
