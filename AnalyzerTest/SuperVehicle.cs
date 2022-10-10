using SlipeLua.Client.Vehicles;
using System.Numerics;

namespace AnalyzerTest
{
    public class SuperVehicle : Vehicle
    {
        public SuperVehicle(VehicleModel model, Vector3 position)
          : base(model, position)
        {
            DamageProof = true;
            Handling.MaxVelocity = 300f;
        }
    }
}