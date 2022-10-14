using SlipeLua.Client.GameWorld;
using SlipeLua.Client.Vehicles;
using SlipeLua.Shared.Attributes;
using System.Numerics;

namespace AnalyzerTest.Foo
{
    public class Program
    {
        [ClientEntryPoint]
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello world!");
            var worldObject = new WorldObject(321, new Vector3(0, 0, 5));
            var vehicle = new Vehicle(VehicleModel.Cars.Alpha, new Vector3(0, 0, 3));
        }
    }
}