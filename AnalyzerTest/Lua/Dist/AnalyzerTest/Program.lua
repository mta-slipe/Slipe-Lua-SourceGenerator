-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaClientGameWorld = SlipeLua.Client.GameWorld
local SlipeLuaClientVehicles = SlipeLua.Client.Vehicles
local SlipeLuaVehicleModel = SlipeLua.Client.Vehicles.VehicleModel
local SystemNumerics = System.Numerics
System.namespace("AnalyzerTest.Foo", function (namespace)
  namespace.class("Program", function (namespace)
    local Main
    Main = function (args)
      System.Console.WriteLine("Hello world!")
      local worldObject = System.new(SlipeLuaClientGameWorld.WorldObject, 2, 321, SystemNumerics.Vector3(0, 0, 5))
      local vehicle = System.new(SlipeLuaClientVehicles.Vehicle, 2, SlipeLuaVehicleModel.Cars.getAlpha(), SystemNumerics.Vector3(0, 0, 3))
    end
    return {
      Main = Main,
      __metadata__ = function (out)
        return {
          methods = {
            { "Main", 0x10E, Main, System.Array(System.String) }
          },
          class = { 0x6 }
        }
      end
    }
  end)
end)
