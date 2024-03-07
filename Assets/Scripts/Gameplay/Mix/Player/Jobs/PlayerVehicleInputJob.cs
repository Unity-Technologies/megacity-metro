using Unity.Burst;
using Unity.Entities;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Job to collect the player input and send it to the player vehicle.
    /// </summary>
    [BurstCompile]
    public partial struct PlayerVehicleInputJob : IJobEntity
    {
        public PlayerVehicleInput CollectedInput;

        [BurstCompile]
        private void Execute(in PlayerVehicleSettings vehicleSettings, ref PlayerVehicleInput inputs)
        {
            if (vehicleSettings.InvertPitch)
            {
                CollectedInput.LookVelocity.x = -CollectedInput.LookVelocity.x;
            }

            inputs = CollectedInput;
        }
    }
}
