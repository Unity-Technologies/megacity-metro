using UnityEngine;

public class FeatureDisabler : MonoBehaviour
{
    void Start()
    {
#if UNITY_SWITCH
        gameObject.gameObject.SetActive(false);
#endif
    }
}
