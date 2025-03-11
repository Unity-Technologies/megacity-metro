using UnityEngine;

public class DisplaySettings : MonoBehaviour
{
    public enum DynamicResolutionControls { off, uiSlider, adaptivePerformance, fromFunctionCalls}
    const float DR_FUNCTION_DRIVEN_TRANSITION_TIME = 3f;

    [SerializeField] int targetFramerate = 30;
    [SerializeField] bool keepScreenAwake = true;
    [SerializeField] DynamicResolutionControls dynamicResolutionControls = DynamicResolutionControls.uiSlider;

    SimpleFPS uiOverlay;

    bool transitioning = false;
    float startingDRValue = 1f, currentDRValue = 1f, targetDRValue = 1f;
    float elapsedTime;

    private void Awake()
    {
        Application.targetFrameRate = targetFramerate;
        if (keepScreenAwake)
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start()
    {
        if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
            dynamicResolutionControls = DynamicResolutionControls.off;

        uiOverlay = FindAnyObjectByType<SimpleFPS>();
        if (uiOverlay != null )
        {
            uiOverlay.SetDRPanelVisibility(dynamicResolutionControls != DynamicResolutionControls.off);
            uiOverlay.SetDRSliderVisibility(dynamicResolutionControls == DynamicResolutionControls.uiSlider);
        }
    }

    private void Update()
    {
        switch (dynamicResolutionControls)
        {
            case DynamicResolutionControls.off:
                return;
            case DynamicResolutionControls.uiSlider:
                if (uiOverlay != null)
                    ApplyDR(uiOverlay.GetDRSliderValue());
                break;
            case DynamicResolutionControls.adaptivePerformance:
                ApplyDR(ScalableBufferManager.widthScaleFactor, false);
                break;
            case DynamicResolutionControls.fromFunctionCalls:
                if (transitioning)
                {
                    float drValue;
                    elapsedTime += Time.deltaTime;
                    float prct = elapsedTime / DR_FUNCTION_DRIVEN_TRANSITION_TIME;
                    if (prct < 1f)
                        drValue = Mathf.Lerp(startingDRValue, targetDRValue, prct);
                    else
                    {
                        drValue = targetDRValue;
                        transitioning = false;
                    }
                    ApplyDR(drValue);
                }
                break;
        }
    }

    float ApplyDR(float drValue, bool rescaleBuffer = true)
    {
        drValue = Mathf.Max(drValue, 0.25f);
        drValue = Mathf.Round(drValue * 100f) * 0.01f;

        if (!Mathf.Approximately(drValue, currentDRValue))
        {
            currentDRValue = drValue;
            uiOverlay?.SetDRSliderValue(currentDRValue);
            if (rescaleBuffer)
                ScalableBufferManager.ResizeBuffers(currentDRValue, currentDRValue);
        }

        return drValue;
    }

    public void SetDRValue(float newValue)
    {
        newValue = Mathf.Max(newValue, 0.25f);
        newValue = Mathf.Min(newValue, 1f);
        startingDRValue = currentDRValue;
        targetDRValue = newValue;
        elapsedTime = 0f;
        transitioning = true;
    }
}
