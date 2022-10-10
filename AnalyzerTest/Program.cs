using SlipeLua.Client.GameWorld;
using SlipeLua.Client.Peds;
using SlipeLua.Client.Vehicles;
using System.Numerics;
using SlipeLua.Shared.Attributes;

namespace AnalyzerTest
{
    public class Program
    {
        [ClientEntryPoint]
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello world!");
            var worldObject = new WorldObject(321, new Vector3(0, 0, 5));
            var vehicle = new SuperVehicle(VehicleModel.Cars.Alpha, new Vector3(0, 0, 3));
        }
    }
}