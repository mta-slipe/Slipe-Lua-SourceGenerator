namespace SlipeLua.CodeGenerator
{
    public class CodeGenerationPatches
    {
        public static string All => $@"
function prePatches()
    {string.Join("\n", PrePatches)}
end

local patches = {{}}

function duringPatches(t, key, value)

    { string.Join("\n", DuringPatches)}

    rawset(t, key, value)
end

function postPatches()
    {string.Join("\n", PostPatches)}
end

prePatches()
";

        public static string[] PrePatches => new string[]
        {
            InitBitApi,
            EmptyRequire,
            AddGlobalMetatable
        };

        public static string[] DuringPatches => new string[]
        {
            SystemIsForElements
        };

        public static string[] PostPatches => new string[]
        {
            RemoveGlobalMetatable
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

        public static string EmptyRequire => @"
    function require() end";

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

        public static string AddGlobalMetatable = @"
    setmetatable(_G, {
        __newindex = function(...) 
            duringPatches(...)
        end
    })";

        public static string SystemIsForElements => @"    
    if (not patches.systemIs and key == ""System"") then
        if (not getmetatable(value)) then
            setmetatable(value, {})
        end

        getmetatable(value).__newindex = function(systemT, systemKey, systemValue)

            if systemKey == ""is"" then
                local function localIs(obj, T)
                    return type(obj) == ""userdata"" or systemValue(obj, T)
                end

                rawset(systemT, systemKey, localIs)
                patches.systemIs = true
                return
            end

            rawset(systemT, systemKey, systemValue)
        end

        patches.system = true
    end";

        public static string RemoveGlobalMetatable = @"
    setmetatable(_G, nil)";
    }
}
