using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
#if ADAPTIVE_PERF
using UnityEngine.AdaptivePerformance;
#endif
using UnityEngine.Rendering;
using FrameTiming = UnityEngine.FrameTiming;

public class CaptureScript : MonoBehaviour
{
    public enum Category
    {
        // Memory
        TotalUsedMemory,
        GCUsedMemory,
        SystemUsedMemory,
        GfxMem,
        TexMem,

        // Render
        Triangles,
        Vertices,
        Batches,
        SetPassCalls,

        DrawCall,
        CpuFrameTime, //FrameTimingManager
        GpuFrameTime, //FrameTimingManager
        CpuMainThreadFrameTime,
        CpuRenderThreadFrameTime
    }

    [SerializeField] float minimumTimeBetweenCaptures = 4f;
    [SerializeField, Tooltip("In debug builds, screenshots will be captured when this many stat captures have occurred. Set to 0 to disable.")] int screenCaptureCadence = 0;
    public short FpsSampleCount = 30;

    private FrameTiming[] _frameTimings;
    public AverageFps AverageFps = new AverageFps();
    public Dictionary<Category, ProfilerRecorder> RecorderMap = new Dictionary<Category, ProfilerRecorder>();

    private string _filename = "statsCapture_";
    private bool IsVulkan = false;
    private bool HasDR = false;
    private string csvFilename;
    private float timeToNextCapture;
    private int captureCount = 0;

    private static CaptureScript instance;
    public static CaptureScript Instance => instance;

    #region Adaptive_Performance

#if ADAPTIVE_PERF
    private IAdaptivePerformance _adaptivePerformance;
#endif
    public bool AdaptiveEnabled
    {
        get
        {
#if ADAPTIVE_PERF
            var apActive = _adaptivePerformance is { Active: true };
            return apActive;
#else
            return false;
#endif
        }
    }

    public float TemperatureLevel
    {
        get
        {
#if ADAPTIVE_PERF

            return AdaptiveEnabled ? _adaptivePerformance.ThermalStatus.ThermalMetrics.TemperatureLevel : -1;
#else
            return -1;
#endif
        }
    }

    public int TemperatureStatus
    {
        get
        {
#if ADAPTIVE_PERF

            return (AdaptiveEnabled ? (int)_adaptivePerformance.ThermalStatus.ThermalMetrics.WarningLevel : -1);
#else
            return -1;
#endif
        }
    }

    public string PerformanceBottleneck
    {
        get
        {
#if ADAPTIVE_PERF

            return Convert.ToString((AdaptiveEnabled
                ? _adaptivePerformance.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck
                : "-"));
#else
            return "-";
#endif
        }
    }

    #endregion

    private void Awake()
    {
        _frameTimings = new FrameTiming[FpsSampleCount];

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
            return;
        }
        
        AverageFps.Initialize(FpsSampleCount);
#if ADAPTIVE_PERF
        _adaptivePerformance = Holder.Instance;
        if (_adaptivePerformance == null || !_adaptivePerformance.Active)
        {
            Debug.LogWarning("Adaptive Performance not active");
        }
#endif
        IsVulkan = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan;
        if (Camera.main != null) HasDR = Camera.main.allowDynamicResolution;

        timeToNextCapture = minimumTimeBetweenCaptures;
    }

    void OnEnable()
    {
        RecorderMap.TryAdd(Category.TotalUsedMemory,
            ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory"));
        RecorderMap.TryAdd(Category.GCUsedMemory,
            ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory"));
        RecorderMap.TryAdd(Category.SystemUsedMemory,
            ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory"));
        RecorderMap.TryAdd(Category.Triangles,
            ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count"));
        RecorderMap.TryAdd(Category.Vertices, ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count"));
        RecorderMap.TryAdd(Category.Batches, ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count"));
        RecorderMap.TryAdd(Category.SetPassCalls,
            ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count"));

        RecorderMap.TryAdd(Category.DrawCall,
            ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count"));
        RecorderMap.TryAdd(Category.CpuMainThreadFrameTime,
            ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread"));
        RecorderMap.TryAdd(Category.CpuRenderThreadFrameTime,
            ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread"));

        RecorderMap.TryAdd(Category.GfxMem, ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory"));
        RecorderMap.TryAdd(Category.TexMem, ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory"));
    }

    void OnDisable()
    {
        foreach (var recorder in RecorderMap.Values)
            recorder.Dispose();

        RecorderMap.Clear();
    }


    private void Update()
    {
        FrameTimingManager.CaptureFrameTimings();
        var deltaTime = Time.deltaTime;
        var fps = 1f / deltaTime;
        AverageFps.Update(fps);

        timeToNextCapture -= Time.deltaTime;
    }

    public static void OnPassWaypoint()
    {
        if (instance == null || instance.timeToNextCapture > 0) return;
        if (!instance.enabled) return;

        instance.timeToNextCapture = instance.minimumTimeBetweenCaptures;
        instance.CaptureStats(instance.captureCount++);
    }


    void CaptureStats(int currentWaypointIndex)
    {
#if DEBUG
        if (screenCaptureCadence > 0 && currentWaypointIndex % screenCaptureCadence == 0)
            TakeScreenshot(currentWaypointIndex);
#endif

        var frames = FrameTimingManager.GetLatestTimings((uint)_frameTimings.Length, _frameTimings);

        var waypointData = currentWaypointIndex;
        var fpsData = AverageFps.averageFps;

        var timeData = Time.realtimeSinceStartup;
        var cpuData = (float)GetAvgFrameTiming(frames, 0, _frameTimings);
        var gpuData = (float)GetAvgFrameTiming(frames, 1, _frameTimings);
        var mainData = (float)GetAvgFrameTiming(frames, 2, _frameTimings);
        var renderData = (float)GetAvgFrameTiming(frames, 3, _frameTimings);

        RecorderMap.TryGetValue(Category.SetPassCalls, out var passCalls);
        RecorderMap.TryGetValue(Category.DrawCall, out var drawCalls);
        RecorderMap.TryGetValue(Category.TotalUsedMemory, out var memory);
        RecorderMap.TryGetValue(Category.TexMem, out var texMemory);
        RecorderMap.TryGetValue(Category.GfxMem, out var gfxMem);

        var drawSetPass = passCalls.LastValue;
        var drawCallData = drawCalls.LastValue;
        var memData = memory.LastValue / (1024 * 1024);
        var texMemData = texMemory.LastValue / (1024 * 1024);
        var gfxMemData = gfxMem.LastValue / (1024 * 1024);

        var temperatureLevelData = TemperatureLevel;
        var thermalStatusData = TemperatureStatus;

        var dynamicResolutionData = IsVulkan && HasDR ? ScalableBufferManager.widthScaleFactor : -1;
        var targetFpsData = Application.targetFrameRate;


        if (string.IsNullOrEmpty(csvFilename))
        {
            var filename = _filename + DateTime.Now.ToFileTime();
            csvFilename = Application.persistentDataPath + "/" + filename + ".csv";
            var header = "Time, Waypoint, Fps, Cpu, Gpu, Main thread, Render thread, " +
                         "Draw calls, SetPass, GFX mem, Tex mem, MB, Temperature level, Thermal status, DR, Target Fps" +
                         System.Environment.NewLine;
            File.AppendAllText(csvFilename, header);
        }

        if (File.Exists(csvFilename))
        {
            var result = new StringBuilder();
            result.Append(timeData + ",");
            result.Append(waypointData + ",");
            result.Append(fpsData + ",");
            result.Append(cpuData + ",");
            result.Append(gpuData + ",");
            result.Append(mainData + ",");
            result.Append(renderData + ",");
            result.Append(drawCallData + ",");
            result.Append(drawSetPass + ",");
            result.Append(gfxMemData + ",");
            result.Append(texMemData + ",");
            result.Append(memData + ",");
            result.Append((temperatureLevelData >= 0 ? $"{temperatureLevelData * 100:F0}" : "-") + ",");
            result.Append(
#if ADAPTIVE_PERF
                thermalStatusData >= 0
                    ? $"{(WarningLevel)thermalStatusData}"
                    :
#endif
                    "-");
            result.Append(",");
            result.Append(dynamicResolutionData + ",");
            result.Append(targetFpsData);
            result.AppendLine();

            File.AppendAllText(csvFilename, result.ToString());
        }
    }

    private void TakeScreenshot(int currentWaypointIndex)
    {
        var filename = _filename + currentWaypointIndex + "_" + DateTime.Now.ToFileTime();
        ScreenCapture.CaptureScreenshot(filename + ".png");
    }

    private double GetAvgFrameTiming(uint count, int type, FrameTiming[] frameTiming)
    {
        double result = 0;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case 0:
                        result += frameTiming[i].cpuFrameTime;
                        break;
                    case 1:
                    {
                        result += frameTiming[i].gpuFrameTime;
                        break;
                    }
                    case 2:
                        result += frameTiming[i].cpuMainThreadFrameTime;
                        break;
                    case 3:
                        result += frameTiming[i].cpuRenderThreadFrameTime;
                        break;
                }
            }

            result /= count;
        }

        return result;
    }
}