using Unity.Mathematics;
using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    public enum GameMode
    {
        None,
        SinglePlayer,
        Multiplayer
    }
    
    [CreateAssetMenu(fileName = "PlayerInfoItemSettings", menuName = "Gameplay/Settings/HUDPlayerInfoSettings", order = 1)]
    public class PlayerInfoItemSettings : ScriptableObject
    {
        [HideInInspector] 
        public string PlayerName;
        public GameMode GameMode;
        [field: SerializeField] public float RayOffsetFromCamera { private set; get; }
        [field: SerializeField] public float MinDistanceBetweenCameraRayAndPlayer { private set; get; }
        [field: SerializeField]  public float MinLifeBar { private set; get; }
        [field: SerializeField]  public float3 MinOffset { private set; get; }
        [field: SerializeField]  public float3 Offset { private set; get; }
        
        public float MinDistanceToShowPlayerInfo = 250f;
        [SerializeField]
        private float2 m_MinMaxScale;
        [SerializeField]
        private float2 m_MinMaxDistance;
        public float MinScale => m_MinMaxScale.x;
        public float MaxScale => m_MinMaxScale.y;
        public float MinDistanceSq => m_MinMaxDistance.x * m_MinMaxDistance.x;
        public float MaxDistanceSq => m_MinMaxDistance.y * m_MinMaxDistance.y;
    }
}
