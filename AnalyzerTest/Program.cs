using SlipeLua.Server.GameWorld;
using SlipeLua.Server.Peds;
using SlipeLua.Server.Vehicles;
using System.Numerics;
using SlipeLua.Shared.Attributes;

namespace AnalyzerTest
{
    public class Program
    {
        [ServerEntryPoint]
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello world!");
            var worldObject = new WorldObject(321, new Vector3(0, 0, 5));
            var vehicle = new SuperVehicle(VehicleModel.Cars.Alpha, new Vector3(0, 0, 3));

            Player.OnJoin += HandlePlayerJoin;
        }

        private static void HandlePlayerJoin(Player source, SlipeLua.Server.Peds.Events.OnJoinEventArgs eventArgs)
        {
            source.Camera.Target = source;
            source.Camera.Fade(SlipeLua.Shared.Rendering.CameraFade.In);
            source.Spawn(new Vector3(5, 0, 3), SlipeLua.Shared.Peds.PedModel.army);
        }
    }
}