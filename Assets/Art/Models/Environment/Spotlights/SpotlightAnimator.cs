using Unity.Mathematics;
using UnityEngine;

namespace Unity.MegacityMetro
{
    public class SpotlightAnimator : MonoBehaviour
    {
        public Spot[] Lights = new Spot[0];
        [SerializeField] private GameObject[] LightModels = new GameObject[4];

        private void OnValidate()
        {
            for (int i = 0; i < LightModels.Length; i++)
            {
                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;
                GameObject instance;
                try
                {
                    instance = LightModels[i];
                }
                catch (UnassignedReferenceException)
                {
                    continue;
                }

                if (instance == null) continue;
                instance.SetActive(i < Lights.Length);

                if (i < Lights.Length)
                {
                    pos = Lights[i].Position;
                    rot = Quaternion.FromToRotation(Vector3.down, Lights[i].ConeVector);
                }

                instance.transform.localPosition = pos;
                instance.transform.localRotation = rot;
            }
        }

        private void Update()
        {
            for (int i = 0; i < Lights.Length; i++)
            {
                var yaw = Mathf.Sin((i * 1.17f) + (Time.time * 1.13f / Lights[i].Period.x)) * Lights[i].ConeAngle.x *
                          Mathf.Deg2Rad;
                var pitch = Mathf.Cos((i * 1.13f) + (Time.time * 0.71f / Lights[i].Period.y)) * Lights[i].ConeAngle.y *
                            Mathf.Deg2Rad;

                LightModels[i].transform.localRotation =
                    quaternion.Euler(pitch, yaw, 0) * Quaternion.FromToRotation(Vector3.down, Lights[i].ConeVector);
            }
        }
    }

    [System.Serializable]
    public struct Spot
    {
        public Vector3 Position;
        public Vector3 ConeVector;
        public Vector3 ConeAngle;
        public Vector3 Period;
    }
}