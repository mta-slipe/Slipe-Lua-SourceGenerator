namespace CodeGenerator
{
    public class CodeGenerationPatches
    {
        public static string All => $@"
function prePatches()
    {string.Join("\n", PrePatches)}
end

function duringPatches()
    {string.Join("\n", DuringPatches)}
end

function postPatches()
    {string.Join("\n", PostPatches)}
end
";

        public static string[] PrePatches => new string[]
        {
            InitBitApi,
            CSharpLuaLoadStringRequire
        };

        public static string[] DuringPatches => new string[]
        {
            SystemIsForElements
        };

        public static string[] PostPatches => new string[]
        {
            
        };

        public static string InitBitApi => @"
    bit = {
        bnot = bitNot,
        band = bitAnd,
        bor = bitOr,
        bxor = bitXor,
        lshift = bitLShift,
        rshift = bitRShift
    }";

        public static string CSharpLuaLoadStringRequire => @"
    function require(path)
        local slashedPath = path:gsub(""%."", ""/"")
        local unslashedBeginPath = slashedPath:gsub(""CoreSystem/Lua"", ""CoreSystem.Lua"")
        local fixedExtensionPath = unslashedBeginPath:gsub(""/lua"", "".lua"")
        local addedExtensionPath = fixedExtensionPath .. "".lua""
        local fullpath = addedExtensionPath:gsub("".lua.lua"", "".lua"")
        local file = fileOpen(fullpath)
        local content = fileRead(file, fileGetSize(file))
        fileClose(file)
        local comment = ""--[["" .. path .. ""]]""
        local result = loadstring(comment..content)()
        duringPatches()
        return result
    end
";

        public static string SystemIsForElements => @"
    isSystemIsPatched = false
    if (not isSytemIsPatched and System and System.is) then
        local oldIs = System.is;
        local function is(obj, T)
            return type(obj) == ""userdata"" and T == SlipeLua.MtaDefinitions.MtaElement or oldIs(obj, T)
        end
        System.is = is
        isSytemIsPatched = true
    end";
    }
}
