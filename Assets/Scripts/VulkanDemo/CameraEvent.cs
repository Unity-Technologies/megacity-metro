using UnityEngine;

[CreateAssetMenu(fileName = "CameraEvent", menuName = "Scriptable Objects/CameraEvent")]
public class CameraEvent : ScriptableObject
{
    public bool modifyFocusTarget = false;
    public int focusTargetIndex = 0;
    public float focusTargetTransitionTime = 1f;
    public bool modifyDistance = false;
    public float newDistance = 10f;
    public float distanceTransitionSpeed = 1f;
    public bool modifyYaw = false;
    public float newYaw = 0f;
    public float yawTransitionSpeed = 1f;
    public bool modifyPitch = false;
    public float newPitch = 0f;
    public float pitchTransitionSpeed = 1f;

}
