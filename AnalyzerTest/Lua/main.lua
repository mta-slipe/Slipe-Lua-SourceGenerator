

prePatches()
require("CoreSystem.Lua/All.lua")("CoreSystem.Lua")
postPatches()
require("Lua/Dist/SlipeMTADefinitions/manifest.lua")("Lua/Dist/SlipeMTADefinitions")
require("Lua/Dist/SlipeShared/manifest.lua")("Lua/Dist/SlipeShared")
require("Lua/Dist/SlipeServer/manifest.lua")("Lua/Dist/SlipeServer")
require("Lua/Dist/SlipeSql/manifest.lua")("Lua/Dist/SlipeSql")
require("Lua/Dist/AnalyzerTest/manifest.lua")("Lua/Dist/AnalyzerTest")

initEvents()
AnalyzerTest.Program.Main()

