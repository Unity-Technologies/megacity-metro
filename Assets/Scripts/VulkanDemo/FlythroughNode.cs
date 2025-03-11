using UnityEngine;
using UnityEngine.Events;

public class FlythroughNode : MonoBehaviour
{
    public float entryCurveStrength = 1f;
    public float exitCurveStrength = 1f;
    public bool modifyShipSpeed = false;
    public float speed = 0f;
    public float timeToSpeed = 1f;
    public bool modifyShipAngle = false;
    public float roll = 0f;
    public float timeToRoll = 0f;
    public CameraEvent cameraChange;
    public UnityEvent actionTriggers;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Transform thisTransform = transform;
        Vector3 forwardPoint = thisTransform.position + thisTransform.forward * exitCurveStrength;
        Vector3 backwardPoint = thisTransform.position - thisTransform.forward * entryCurveStrength;
        Gizmos.DrawLine(backwardPoint, forwardPoint);
        Gizmos.DrawWireSphere(forwardPoint, 0.5f);
        Gizmos.DrawWireSphere(backwardPoint, 0.5f);
    }
#endif
}
