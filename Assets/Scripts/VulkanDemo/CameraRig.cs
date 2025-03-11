using UnityEngine;

public class CameraRig : MonoBehaviour
{
    [SerializeField] Transform[] focalPointTransforms;

    Transform currentFocalPointTransform;
    Vector3 startingFocalPoint, currentFocalPoint, targetFocalPoint;
    float focalPointTransitionDuration, focalPointTransitionElapsedTime;

    float distanceUpdateSpeed, tiltUpdateSpeed, yawUpdateSpeed;

    float currentDistance, targetDistance;
    float currentTilt, targetTilt;
    float currentYaw, targetYaw;
    float targetYawOffset;

    Transform thisTransform;
    static CameraRig instance;

    private void Awake()
    {
        if (instance != null && instance != this)
            Debug.LogWarning("Multiple camera rigs found in scene", this);
        else
            instance = this;
    }

    void Start()
    {
        thisTransform = transform;
        InitializeValues();
    }

    void LateUpdate()
    {
        UpdateValues();
        UpdatePosition();
    }

    public static void ApplyCameraEvent(CameraEvent cameraEvent)
    {
        if (instance == null) return;
        instance.SetCameraParameters(cameraEvent);
    }

    void SetCameraParameters(CameraEvent cameraEvent)
    {
        if (cameraEvent.modifyFocusTarget)
        {
            if (cameraEvent.focusTargetIndex > -1 && cameraEvent.focusTargetIndex < focalPointTransforms.Length)
                currentFocalPointTransform = focalPointTransforms[cameraEvent.focusTargetIndex];
            else
                currentFocalPointTransform = null;

            focalPointTransitionDuration = cameraEvent.focusTargetTransitionTime;
            focalPointTransitionElapsedTime = 0;
            startingFocalPoint = currentFocalPoint;
        }

        if (cameraEvent.modifyDistance)
        {
            targetDistance = cameraEvent.newDistance;
            distanceUpdateSpeed = cameraEvent.distanceTransitionSpeed;
        }

        if (cameraEvent.modifyYaw)
        {
            targetYawOffset = cameraEvent.newYaw;
            yawUpdateSpeed = cameraEvent.yawTransitionSpeed;
        }

        if (cameraEvent.modifyPitch)
        {
            targetTilt = cameraEvent.newPitch;
            tiltUpdateSpeed = cameraEvent.pitchTransitionSpeed;
        }
    }

    void UpdateValues()
    {
        while (currentYaw < -180f)
        {
            currentYaw += 360f;
            targetYaw += 360f;
        }
        while (currentYaw > 180f)
        {
            currentYaw -= 360f;
            targetYaw -= 360f;
        }

        if (currentFocalPointTransform != null)
        {
            targetFocalPoint = currentFocalPointTransform.position;
            targetYaw = currentFocalPointTransform.eulerAngles.y + targetYawOffset;
            while (targetYaw + 180f < currentYaw)
                targetYaw += 360f;
            while (targetYaw - 180f > currentYaw)
                targetYaw -= 360f;
        }

        currentDistance = FramerateIndependentLerp(currentDistance, targetDistance, Time.deltaTime, distanceUpdateSpeed);
        currentTilt = FramerateIndependentLerp(currentTilt, targetTilt, Time.deltaTime, tiltUpdateSpeed);
        currentYaw = FramerateIndependentLerp(currentYaw, targetYaw, Time.deltaTime, yawUpdateSpeed);

        if (focalPointTransitionDuration > 0)
        {
            focalPointTransitionElapsedTime += Time.deltaTime;
            float percent = focalPointTransitionElapsedTime / focalPointTransitionDuration;
            if (percent >= 1f)
            {
                currentFocalPoint = targetFocalPoint;
                focalPointTransitionDuration = 0;
            } else
            {
                currentFocalPoint = Vector3.Lerp(startingFocalPoint, targetFocalPoint, EaseOut(percent));
            }
        } else
            currentFocalPoint = targetFocalPoint;
    }

    float EaseOut(float value)
    {
        value = 1f - value;
        value = value * value;
        return 1f - value;
    }

    void UpdatePosition()
    {
        Quaternion rotation = Quaternion.Euler(currentTilt, currentYaw, 0);
        thisTransform.position = currentFocalPoint + rotation * new Vector3(0, 0, -currentDistance);
        thisTransform.rotation = rotation;
    }

    void InitializeValues()
    {
        if (focalPointTransforms == null || 
            focalPointTransforms.Length == 0 ||
            focalPointTransforms[0] == null) return;

        currentFocalPointTransform = focalPointTransforms[0];
        currentFocalPoint = currentFocalPointTransform.position;
        currentDistance = Vector3.Magnitude(currentFocalPoint - thisTransform.position);
        currentTilt = thisTransform.eulerAngles.x;
        currentYaw = thisTransform.eulerAngles.y;

        targetDistance = currentDistance;
        targetTilt = currentTilt;
        targetYaw = currentYaw;
        targetYawOffset = 0;
        focalPointTransitionDuration = 0;

        distanceUpdateSpeed = 5f;
        tiltUpdateSpeed = 5f;
        yawUpdateSpeed = 5f;
    }

    float FramerateIndependentLerp(float from, float to, float deltaTime, float lambda)
    {
        return Mathf.Lerp(from, to, 1f - Mathf.Exp(-lambda * deltaTime));
    }
}
