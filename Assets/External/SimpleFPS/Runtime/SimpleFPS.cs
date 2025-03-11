using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


public class SimpleFPS : MonoBehaviour
{
    public UIDocument MainPanel;
    public float UiDelay = 0.01f;

    float uiTimer;

    private bool IsVulkan = false;

    private FloatAverage fpsAvg;
    private ProfilerRecorder cpu;
    private ProfilerRecorder drawCalls;

    public float drValue
    {
        get => _drSlider.value;
        set => _drSlider.value = value;
    }

    private Label _fpsLabel;
    private Label _cpuLabel;
    private Label _drawCallsLabel;
    private Label _drLabel;
    private Slider _drSlider;
    private VisualElement _drPanel;

    private void Awake()
    {
        var root = MainPanel.rootVisualElement;
        _fpsLabel = root.Q<Label>("FPS-Label");
        _cpuLabel = root.Q<Label>("CPU-Label");
        _drawCallsLabel = root.Q<Label>("DrawCalls-Label");
        _drLabel = root.Q<Label>("DR-Value");
        _drSlider = root.Q<Slider>("DR-Slider");
        _drPanel = root.Q<VisualElement>("DR-Panel");
    }

    void Start()
    {
        var root = MainPanel.rootVisualElement;
        var fpsPanel = root.Q<VisualElement>("FPS-Panel");
        var apiLabel = root.Q<Label>("Api-Label");

        string graphicApi;
        switch (SystemInfo.graphicsDeviceType)
        {
            case GraphicsDeviceType.Vulkan:
                graphicApi = "VULKAN";
                IsVulkan = true;
                break;
            case GraphicsDeviceType.OpenGLES3:
                graphicApi = "OPENGL ES";
                break;
            default:
                graphicApi = "Others";
                break;
        }

        apiLabel.text = graphicApi;
        if (!IsVulkan)
        {
            fpsPanel.style.alignSelf = Align.FlexStart;
            fpsPanel.style.bottom = 0;
        }

        fpsAvg = new FloatAverage();
        cpu = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 30);
        drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
    }

    void Update()
    {
        var deltaTime = Time.deltaTime;
        var fps = 1f / deltaTime;
        fpsAvg.Add(fps);

        if (uiTimer < Time.realtimeSinceStartup)
        {
            uiTimer = Time.realtimeSinceStartup + UiDelay;
            _fpsLabel.text = $"{fpsAvg.GetAverage():F1}";
            _cpuLabel.text = $"{GetRecorderFrameAverage(cpu) * (1e-6f):F2}";
            _drawCallsLabel.text = $"{drawCalls.LastValue}";
        }
    }

    public void SetDRPanelVisibility(bool visible)
    {
        _drPanel.visible = visible;
    }
    public void SetDRSliderVisibility(bool visible)
    {
        _drSlider.visible = visible;
    }

    public float GetDRSliderValue()
    {
        return _drSlider.value;
    }

    public void SetDRSliderValue(float value)
    {
        _drSlider.value = value;
        _drLabel.text = $"{value:P0}";
    }

    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        for (int i = 0; i < recorder.Count; i++)
        {
            r += recorder.GetSample(i).Value;
        }

        return r / recorder.Count;
    }
}