using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] float thrusterBaseScale = 0.7f;
    [SerializeField] Transform[] thrusters;
    [SerializeField] ParticleSystem[] trails;

    public void SetThrusterPower(float percent)
    {
        Vector3 localScale = new Vector3(thrusterBaseScale, thrusterBaseScale, thrusterBaseScale * percent);
        for (var i = 0; i < thrusters.Length; i++)
        {
            thrusters[i].localScale = localScale;
        }
    }
}
