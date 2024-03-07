using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Capture the user input and apply them to a component for later uses
    /// </summary>
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerVehicleInput : IInputComponentData
    {
        public float2 LookVelocity;
        public float AimAssistSensitivity;
        public float Acceleration; // acceleration
        public float Roll; // manual roll
        public bool Shoot; // Shoot laser (X or A)

        // Cheats
        public bool Cheat_1; // Auto-hurt
    }
}
