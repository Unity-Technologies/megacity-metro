using UnityEngine;

public class TrackFlier : MonoBehaviour
{
    public const int BEZIER_SUBDIVISION_COUNT = 16;
    const float ACCELERATION_FACTOR = 5f;

    [Header("Parameters")]
    [SerializeField] Transform[] nodeHolders;
    [SerializeField] bool playOnStart = false;
    [SerializeField] float speedMultiplier = 1f;
    [SerializeField, Range(0, 1f), Tooltip("Heavier ships will slow when ascending and speed up when descending")] float shipWeight = 0.5f;
    [SerializeField, Tooltip("Controls automatic banking as ships go around turns")] float tiltFactor = 0.4f;
    

    [Header("Setup")]
    [SerializeField] Transform tiltTarget;
    [SerializeField, Tooltip("Amount to offset samples on the curve for calculating rotation; prevents abrupt changes")] float smoothingOffset = 80f;
    [SerializeField] ParticleController particleController;
    [SerializeField, Tooltip("For thruster particles, this speed will translate to default particle size/intensity")] float referenceSpeed = 100f;
    [SerializeField] AnimationCurve barrelRollCurve;

    FlythroughNode[][] nodeList;
    int currentFlythroughSet = 0;

    BezierCurve[][] bezierCurves;
    float[][][] segmentLengths;

    CurveSampler currentCurveSampler;
    CurveSampler trailingCurveSampler, leadingCurveSampler;

    float elapsedSpeedTransitionTime, totalSpeedTransitionTime;
    float currentSpeed, startingSpeed, targetSpeed;

    float elapsedRollTransitionTime, totalRollTransitionTime;
    float currentRoll, startingRoll, targetRoll;

    float baseSpeed;

    bool playing = false;

    private void Awake()
    {
        trailingCurveSampler = new CurveSampler(null);
        currentCurveSampler = new CurveSampler(OnNewNode);
        leadingCurveSampler = new CurveSampler(null);
        PopulateCurveData();
    }

    private void Start()
    {
        if (playOnStart) 
            StartPlayback();
    }

    [ContextMenu("Refresh Curve Data")]
    public void PopulateCurveData()
    {
        nodeList = new FlythroughNode[nodeHolders.Length][];
        for (var i = 0; i < nodeHolders.Length; i++)
            nodeList[i] = nodeHolders[i].GetComponentsInChildren<FlythroughNode>();

        bezierCurves = new BezierCurve[nodeHolders.Length][];
        segmentLengths = new float[nodeHolders.Length][][];
        for (var i = 0; i < bezierCurves.Length; i++)
        {
            int curveCount = nodeList[i].Length - 1;
            bezierCurves[i] = new BezierCurve[curveCount];
            segmentLengths[i] = new float[curveCount][];
            Transform currentNodeTransform;
            Transform nextNodeTransform = nodeList[i][0].transform;
            for (var j = 0; j < curveCount; j++)
            {
                currentNodeTransform = nextNodeTransform;
                nextNodeTransform = nodeList[i][j + 1].transform;

                bezierCurves[i][j] = new BezierCurve(
                    currentNodeTransform.position,
                    currentNodeTransform.position + currentNodeTransform.forward * nodeList[i][j].exitCurveStrength,
                    nextNodeTransform.position - nextNodeTransform.forward * nodeList[i][j + 1].entryCurveStrength,
                    nextNodeTransform.position);

                segmentLengths[i][j] = new float[BEZIER_SUBDIVISION_COUNT];
                bezierCurves[i][j].GetApproximateLength(segmentLengths[i][j]);
            }
        }
    }

    void Update()
    {
        if (!playing) return;

        UpdateSpeed();
        float stepSize = CalculateCurrentStepSize();
        trailingCurveSampler.Advance(stepSize);
        currentCurveSampler.Advance(stepSize);
        leadingCurveSampler.Advance(stepSize);
        Vector3 trailingPosition = trailingCurveSampler.SampleCurrentPosition();
        Vector3 centerPosition = currentCurveSampler.SampleCurrentPosition();
        Vector3 leadingPosition = leadingCurveSampler.SampleCurrentPosition();
        ApplyPosition(centerPosition);
        UpdateRoll();
        ApplyRotation(trailingPosition, centerPosition, leadingPosition);
    }

    [ContextMenu("Start Playback")]
    public void StartPlayback()
    {
        StartPlayback(0);
    }

    public void StartPlayback(int nodeSetIndex)
    {
        if (nodeSetIndex < 0 || nodeSetIndex >= nodeList.Length)
            throw new System.ArgumentOutOfRangeException("Invalid node set index: " + nodeSetIndex);
        if (nodeList[nodeSetIndex].Length < 2)
        {
            Debug.LogWarning("Invalid node list");
            playing = false;
            return;
        }

        trailingCurveSampler.MapToSet(bezierCurves[nodeSetIndex], segmentLengths[nodeSetIndex]);
        currentCurveSampler.MapToSet(bezierCurves[nodeSetIndex], segmentLengths[nodeSetIndex]);
        leadingCurveSampler.MapToSet(bezierCurves[nodeSetIndex], segmentLengths[nodeSetIndex]);
        currentCurveSampler.Advance(smoothingOffset);
        leadingCurveSampler.Advance(smoothingOffset * 2f);

        playing = true;
        currentFlythroughSet = nodeSetIndex;
        baseSpeed = nodeList[nodeSetIndex][0].speed;
        if (baseSpeed < 5f) baseSpeed = 100f;
        currentSpeed = baseSpeed;
        totalSpeedTransitionTime = 0;
        currentRoll = nodeList[nodeSetIndex][0].roll;
        totalRollTransitionTime = 0;
        OnNewNode(0);
    }

    void UpdateSpeed()
    {
        if (totalSpeedTransitionTime > 0)
        {
            elapsedSpeedTransitionTime += Time.deltaTime;
            float percent = elapsedSpeedTransitionTime / totalSpeedTransitionTime;
            if (percent >= 1f)
            {
                baseSpeed = targetSpeed;
                totalSpeedTransitionTime = 0;
            }
            else
                baseSpeed = Mathf.Lerp(startingSpeed, targetSpeed, percent);
            particleController?.SetThrusterPower(baseSpeed / referenceSpeed);
        }

        float weightMultiplier = 1f + transform.forward.y * -shipWeight;
        currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed * weightMultiplier, Time.deltaTime * ACCELERATION_FACTOR);
    }

    void UpdateRoll()
    {
        if (totalRollTransitionTime > 0)
        {
            elapsedRollTransitionTime += Time.deltaTime;
            float percent = elapsedRollTransitionTime/ totalRollTransitionTime;
            if (percent >= 1f)
            {
                currentRoll = targetRoll;
                totalRollTransitionTime = 0;
            }
            else
            {
                percent = barrelRollCurve.Evaluate(percent);
                currentRoll = Mathf.LerpUnclamped(startingRoll, targetRoll, percent);
            }
        }
    }

    float CalculateCurrentStepSize()
    {
        return currentSpeed * speedMultiplier * Time.deltaTime;
    }

    void ApplyPosition(Vector3 position)
    {
        transform.position = position;
    }

    void ApplyRotation(Vector3 trailingPosition, Vector3 centerPosition, Vector3 leadingPosition)
    {
        Vector3 direction = leadingPosition - trailingPosition;
        float directionMagnitude = direction.magnitude;
        if (directionMagnitude < smoothingOffset * 0.1f)
        {
            Debug.LogWarning("Bad direction vector: " + trailingPosition + " to "  + leadingPosition);
            return;
        }
        Vector3 right = Vector3.Cross(direction.normalized, Vector3.up);
        Vector3 tiltVector = Vector3.Project(centerPosition - trailingPosition, right);
        float tiltIntensity = (tiltVector.magnitude / directionMagnitude) * Mathf.Sign(Vector3.Dot(tiltVector, right));

        tiltIntensity *= tiltFactor;
        tiltIntensity = Mathf.Max(-90f, tiltIntensity);
        tiltIntensity = Mathf.Min(90f, tiltIntensity);

        transform.rotation = Quaternion.LookRotation(direction);

        tiltTarget.localRotation = Quaternion.Euler(0, 0, tiltIntensity + currentRoll);
    }


    void OnNewNode(int nodeIndex)
    {
        if (nodeIndex >= nodeList[currentFlythroughSet].Length - 1)
        {
            //End of node list, stop playback
            playing = false;
            return;
        }

        FlythroughNode currentNode = nodeList[currentFlythroughSet][nodeIndex];

        if (currentNode.modifyShipSpeed)
        {
            startingSpeed = baseSpeed;
            targetSpeed = currentNode.speed;
            totalSpeedTransitionTime = currentNode.timeToSpeed;
            elapsedSpeedTransitionTime = 0;
            if (totalSpeedTransitionTime <= 0)
                baseSpeed = targetSpeed;
        }

        if (currentNode.modifyShipAngle)
        {
            startingRoll = currentRoll;
            targetRoll = currentNode.roll;
            totalRollTransitionTime = currentNode.timeToRoll;
            elapsedRollTransitionTime = 0;
            if (totalRollTransitionTime <= 0)
                currentRoll = targetRoll;
        }

        if (currentNode.cameraChange != null)
            CameraRig.ApplyCameraEvent(currentNode.cameraChange);

        CaptureScript.OnPassWaypoint();

        currentNode.actionTriggers?.Invoke();
    }
}
