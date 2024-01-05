using UnityEngine;

namespace Unity.Services.Samples
{

    [System.Serializable]
    public class PlayerProfile
    {
        //Decorating with [field: SerializeField] is shorthand for:
        //  public string Name => m_Name;
        //  [SerializeField]
        //  string m_Name;
        [field: SerializeField] public string Name { get; private set; }

        [field: SerializeField] public string UASId { get; private set; }
        public void SetName(string newName)
        {
            Name = newName;
        }

        public PlayerProfile(string name, string uasId)
        {
            Name = name;
            UASId = uasId;
        }

        public override string ToString()
        {
            return $"{Name} ({UASId})";
        }
    }
}
