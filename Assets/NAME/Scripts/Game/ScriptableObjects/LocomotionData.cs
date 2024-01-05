using UnityEngine;

namespace Unity.NAME.Game
{
    // A file that represents the locomotion data
    // One has already been created for the player
    // New ones can be created for different entities (enemies or NPCs)

    [CreateAssetMenu(fileName = "New Locomotion Data", menuName = "ScriptableObject/Locomotion Data")]
    public class LocomotionData : ScriptableObject
    {
        [Header("On Ground Data")]
        [Tooltip("Dampener apply from the CurrentMoveDirection to the DesiredMoveDirection")]
        public float MoveDirectionDampener = 8.0f;
        [Tooltip("Run Speed in meter by second")]
        public float RunSpeed = 4.0f;
        [Range(1.0f, 1080.0f), Tooltip("Rotation Speed")]
        public float AngularSpeed = 720.0f;

        [Header("In Air Data")]
        [Range(0.1f, 1.0f)]
        public float InAirMoveDampener = 0.2f;
        [Range(0.1f, 10.0f)]
        public float GravityMultiplier = 5.0f;
        [Range(0.1f, 30.0f), Tooltip("Vertical force applied on Jump when moving Up")]
        public float JumpForce = 16.0f;
        [Range(0.1f, 30.0f), Tooltip("Vertical force applied on Jump when moving down")]
        public float LowJumpMultiplier = 8.0f;
        [Range(0.1f, 30.0f), Tooltip("Vertical force applied on Fall")]
        public float FallMultiplier = 2.0f;
    }
}
