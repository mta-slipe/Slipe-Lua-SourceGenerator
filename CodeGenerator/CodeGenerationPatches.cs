namespace SlipeLua.CodeGenerator;

public class CodeGenerationPatches
{
    public static string All => $$"""""
        function prePatches()
            {{string.Join("\n", PrePatches)}}
        end

        local patches = {}

        function duringPatches(t, key, value)

            {{ string.Join("\n", DuringPatches)}}

            rawset(t, key, value)
        end

        function postPatches()
            {{ string.Join("\n", PostPatches )}}
        end

        prePatches()
        """"";

    public static string[] PrePatches =>
    [
        InitBitApi,
        EmptyRequire,
        AddGlobalMetatable
    ];

    public static string[] DuringPatches =>
    [
        SystemPatches
    ];

    public static string[] PostPatches =>
    [
        RemoveGlobalMetatable
    ];

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
        local content = fileGetContents(fullpath, true)
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

    public static string SystemPatches => @"    
    if (not patches.system and key == ""System"") then
        if (not getmetatable(value)) then
            setmetatable(value, {})
        end

        getmetatable(value).__newindex = function(systemT, systemKey, systemValue)

            -- allow System.is to work for userdata elements
            if systemKey == ""is"" then
                local function localIs(obj, T)
                    return type(obj) == ""userdata"" or systemValue(obj, T)
                end

                rawset(systemT, systemKey, localIs)
                patches.systemIs = true
                return
            end

            -- Keep track of all assemblies initialised
            if systemKey == ""init"" then
                local function localInit(...)
                    local assembly = systemValue(...)
                    patches.assemblies[#patches.assemblies + 1] = assembly
                    return assembly
                end

                rawset(systemT, systemKey, localInit)
                patches.systemInit = true
                return
            end

            rawset(systemT, systemKey, systemValue)
        end

        -- add support for AppDomain.CurrentDomain.GetAssemblies
        value.AppDomain = {
            getCurrentDomain = function()
                return {
                    GetAssemblies = function()
                        for _, assembly in pairs(patches.assemblies) do
                            setmetatable(assembly, System.Reflection.Assembly)
                        end
                        return System.Array(System.Reflection.Assembly)(#patches.assemblies, patches.assemblies)
                    end
                }
            end
        }

        patches.assemblies = {}
        patches.system = true
    end
";

    public static string RemoveGlobalMetatable = @"
    setmetatable(_G, nil)";
}
