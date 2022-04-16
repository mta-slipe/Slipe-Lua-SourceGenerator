
function prePatches()
    
    bit = {
        bnot = bitNot,
        band = bitAnd,
        bor = bitOr,
        bxor = bitXor,
        lshift = bitLShift,
        rshift = bitRShift
    }

    function require(path)
        local slashedPath = path:gsub("%.", "/")
        local unslashedBeginPath = slashedPath:gsub("CoreSystem/Lua", "CoreSystem.Lua")
        local fixedExtensionPath = unslashedBeginPath:gsub("/lua", ".lua")
        local addedExtensionPath = fixedExtensionPath .. ".lua"
        local fullpath = addedExtensionPath:gsub(".lua.lua", ".lua")
        local file = fileOpen(fullpath)
        local content = fileRead(file, fileGetSize(file))
        fileClose(file)
        local comment = "--[[" .. path .. "]]"
        local result = loadstring(comment..content)()
        duringPatches()
        return result
    end

end

function duringPatches()
    
    isSystemIsPatched = false
    if (not isSytemIsPatched and System and System.is) then
        local oldIs = System.is;
        local function is(obj, T)
            return type(obj) == "userdata" and T == SlipeLua.MtaDefinitions.MtaElement or oldIs(obj, T)
        end
        System.is = is
        isSytemIsPatched = true
    end
end

function postPatches()
    
end
