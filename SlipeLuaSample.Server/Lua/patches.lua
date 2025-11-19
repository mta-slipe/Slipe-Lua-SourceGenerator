function prePatches()
    
    bit = {
        bnot = bitNot,
        band = bitAnd,
        bor = bitOr,
        bxor = bitXor,
        lshift = bitLShift,
        rshift = bitRShift
    }

    function require() end

    setmetatable(_G, {
        __newindex = function(...) 
            duringPatches(...)
        end
    })
end

local patches = {}

function duringPatches(t, key, value)

        
    if (not patches.system and key == "System") then
        if (not getmetatable(value)) then
            setmetatable(value, {})
        end

        getmetatable(value).__newindex = function(systemT, systemKey, systemValue)

            -- allow System.is to work for userdata elements
            if systemKey == "is" then
                local function localIs(obj, T)
                    return type(obj) == "userdata" or systemValue(obj, T)
                end

                rawset(systemT, systemKey, localIs)
                patches.systemIs = true
                return
            end

            -- Keep track of all assemblies initialised
            if systemKey == "init" then
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


    rawset(t, key, value)
end

function postPatches()
    
    setmetatable(_G, nil)
end

prePatches()