using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using System;
using Unity.Profiling;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
[DefaultExecutionOrder(1000)]
public class GfxInfoProfilingTool : MonoBehaviour 
{
    public bool m_Enable = true;

    private GameObject objUI;
    private string currentSceneName;

    //For Realtime
    [Header("For realtime numbers")]
    public int kAverageFrameCount = 32;
    private int frameCount;

	//For Sampling
    [Header("For sampling average")]
    public int sampleSkipCount = 200;
    public int sampleDataCount = 500;
	private int sampleCount;
    private int sampleTotalCount; //skip+sample
	private int sampleStatus = -1;

    //For loadingtime / starting frame
    [Header("For marker starting time / frame")]
    public bool showMarkerStartTiming = false;
    private int accFrameCounter = 0;
    private float accDeltaTime = 0f;

    //For ProfilerRecorder - RenderingModule
    //https://docs.unity3d.com/2023.1/Documentation/Manual/ProfilerRendering.html#markers.html
    private ProfilerRecorder[] profilerRenderingRec;
    private string us_stat_names = "";
    private string us_stat_values = "";
    private string[] profilerRenderingStat = new string[]
    {
        "SetPass Calls Count",
        "Draw Calls Count",
        "Total Batches Count",
        "Used Buffers Count",
        "Used Buffers Bytes",
        "Vertex Buffer Upload In Frame Count",
        "Vertex Buffer Upload In Frame Bytes",
        "Index Buffer Upload In Frame Count",
        "Index Buffer Upload In Frame Bytes",
        "Shadow Casters Count"
    };

    //======================================================================
    //For Render Pipeline Begin End Frame times
    private double renderPipelineBeginTime = 0;
    private double renderPipelineBeginEndTime = 0;

    void Start()
    {
        //Register render pipeline callbacks
        RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
        RenderPipelineManager.endFrameRendering += OnEndFrameRendering;
    }

    void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
    {
        renderPipelineBeginTime = Time.realtimeSinceStartupAsDouble;
    }

    void OnEndFrameRendering(ScriptableRenderContext context, Camera[] cameras)
    {
        renderPipelineBeginEndTime = Time.realtimeSinceStartupAsDouble - renderPipelineBeginTime;
        renderPipelineBeginEndTime *= 1000.0f; //ms
    }

    void OnDestroy()
    {
        RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
        RenderPipelineManager.endFrameRendering -= OnEndFrameRendering;
    }
    //======================================================================

    void Awake()
    {
        if (!Debug.isDebugBuild)
        {
           DestroyImmediate(this);
        }
        QualitySettings.vSyncCount = 0;
        RateSettings.UnlockFramerate = true;
        RateSettings.ApplyFrameRate();
        //Screen.SetResolution(1920, 1080, true);
        //Screen.fullScreen = false;

		sampleCount = 1;
		sampleTotalCount = sampleSkipCount + sampleDataCount;
		sampleStatus = 0;

        currentSceneName = SceneManager.GetActiveScene().name;

        MakeBgTex();

        TimeDataSetUp();
        GeneralInfoListSetup();
        MemoryListSetup();
        TexMemoryListSetup();

        //ProfilerRecorder setup
        profilerRenderingRec = new ProfilerRecorder[profilerRenderingStat.Length];
        for(int i=0; i<profilerRenderingRec.Length;i++)
        {
            profilerRenderingRec[i] = ProfilerRecorder.StartNew(ProfilerCategory.Render, profilerRenderingStat[i]);
        }
    }

    void OnEnable()
    {
        //Frametiming thread
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");
        renderThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "CPU Render Thread Frame Time");
    }

    void OnDisable()
    {
        systemMemoryRecorder.Dispose();

        //ProfilerRecorder cleanup
        for(int i=0; i<profilerRenderingRec.Length;i++)
        {
            profilerRenderingRec[i].Dispose();
        }

        //Frametiming thread
        mainThreadTimeRecorder.Dispose();
        renderThreadTimeRecorder.Dispose();
    }
  
    void Update()
    {
        accDeltaTime += Time.deltaTime;
        
        KeyboardControl();

        if (m_Enable)
        {
            //End of average sampling actions. Put it here to prevent 1 frame delay
            if(sampleStatus == 2 && !captured)
            {
                EndofSampleActions();
            }

            //Update realtime
            TimeDataUpdateRealtime();
         
			//Average sampling
			if(sampleCount > sampleSkipCount)
        	{
                //Done sampling
				if(sampleCount == sampleTotalCount)
				{
					TimeDataDoneSample();
					sampleStatus = 2;
				}
                //Take samples
				else if(sampleCount < sampleTotalCount)
				{
                    TimeDataUpdateSample();
					sampleStatus = 1;
				}
			}

			sampleCount++;
            frameCount++;
            accFrameCounter++;

            //Done realtime
            if (frameCount >= kAverageFrameCount)
            {
                TimeDataDoneRealtime();
                frameCount = 0;
            }
        }
    }

    private void KeyboardControl()
    {
        #if ENABLE_LEGACY_INPUT_MANAGER
            if(Input.GetKeyDown(KeyCode.Alpha7))
            {
                ToggleMiniProfiler();
            }

            if(Input.GetKeyDown(KeyCode.Alpha8))
            {
                ToggleUI();
            }

            if(Input.GetKeyDown(KeyCode.Alpha9))
            {
                PrevScene();
            }

            if(Input.GetKeyDown(KeyCode.Alpha0))
            {
                NextScene();
            }
        #endif
    }

    void OnGUI()
    {
        float scale = Screen.height / 1080f;

        GUI.skin.label.fontSize = Mathf.RoundToInt ( guiFontSize * scale );
        GUI.skin.box.normal.background = bktex;
        ResetGUIBgColor();
        GUI.contentColor = Color.white;
        GUI.color = Color.white;
        int padding = 5;
        //Width
        float w = guiWidth;
        w *= scale;
        //Height
        float h = Screen.height - padding * 2;
        if(!m_Enable) h = 70;
        //Position
        float x;
        switch(guiAlign)
        {
            case GuiAlign.Left: x = padding; break;
            case GuiAlign.Right: x = Screen.width-w-padding; break;
            default: x = (Screen.width-w)*0.5f; break;
        }
        float y = padding;
        GUILayout.BeginArea(new Rect(x, y, w, h), GUI.skin.box);

        //Toggles
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("\n Toggle Miniprofiler (Key 7) \n")) ToggleMiniProfiler();
        if(GUILayout.Button("\n Toggle UI (Key 8) \n")) ToggleUI();
        GUILayout.EndHorizontal();

        if (m_Enable)
        {
            //Scene switches ===============================================================
            // GUILayout.BeginHorizontal();
            // if(GUILayout.Button("\n Prev (Key 9) \n")) PrevScene();
            // if(GUILayout.Button("\n Next (Key 0) \n")) NextScene();
			// GUILayout.EndHorizontal();

            //GeneralInfo ===============================================================
            GeneralInfoListUpdate();
            DataPairDisplay(generalInfoList);

			//Sampling status message =================================================================

            GUILayout.BeginHorizontal();

            //realtime number message
            GUILayout.Label
            (
                CyanText("Cyan numbers are realtime numbers (average of latest "+kAverageFrameCount+" frames).") + "\n" +
                ( gpuRecorder? "" : RedText("\nGPU Recorder is not supported on current Graphics API or hardware.") )  /*+ 
                "\nFrameTiming is only available on DX12/XB1/Switch/PS4/Vulkan/Metal in player. Please enable Frame Timing Stats in PlayerSettings." 
                #if UNITY_EDITOR
                + (UnityEditor.PlayerSettings.enableFrameTimingStats? "" : RedText("\nFrameTimingStats is disabled in PlayerSettings."))
                #endif */
            );

            //sampled number message
            if(labelRightAlignStyle == null )
            {
                labelRightAlignStyle = new GUIStyle(GUI.skin.GetStyle("label"));
                labelRightAlignStyle.alignment = TextAnchor.MiddleRight;

            } 
			switch(sampleStatus)
			{
				case 0: GUILayout.Label(RedText("Skipping first "+ sampleSkipCount +" frames..."+sampleCount+"/"+sampleTotalCount),labelRightAlignStyle); break;
				case 1: GUILayout.Label(YellowText("Now sampling " + sampleDataCount + " frames..."+sampleCount+"/"+sampleTotalCount),labelRightAlignStyle); break;
                case 2: GUILayout.Label(GreenText("Done sampling. Green numbers are average of "+sampleDataCount+" frames. \n(First "+sampleSkipCount+" is skipped and then sampled "+sampleDataCount+" frames.)"),labelRightAlignStyle); break;
			}

            GUILayout.EndHorizontal();

            //TimeData ============================================================
            TimeDataDisplay();

            //UnityStats =========================================================
            /*
            #if UNITY_EDITOR

            GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                string us_title = "<b>UnityStats" + "\n";
                us_title += "Total" + "\n";
                us_title += "Static Batched" + "\n";
                us_title += "Dynamic Batched" + "\n";
                us_title += "Instanced Batched</b>" + "\n";
                GUILayout.Label(us_title);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                string us_drawcall = "<b>DrawCalls</b>" + "\n";
                us_drawcall += UnityStats.drawCalls.ToString() + "\n";
                us_drawcall += UnityStats.staticBatchedDrawCalls.ToString() + "\n";
                us_drawcall += UnityStats.dynamicBatchedDrawCalls.ToString() + "\n";
                us_drawcall += UnityStats.instancedBatchedDrawCalls.ToString() + "\n";
                GUILayout.Label(us_drawcall);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                string us_batch = "<b>Batches</b>" + "\n";
                us_batch += UnityStats.batches.ToString() + "\n";
                us_batch += UnityStats.staticBatches.ToString() + "\n";
                us_batch += UnityStats.dynamicBatches.ToString() + "\n";
                us_batch += UnityStats.instancedBatches.ToString() + "\n";
                GUILayout.Label(us_batch);
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                string us_meta = "<b>setPassCalls" + "\n";
                us_meta += "renderTextureBytes / Changes / Count" + "\n";
                us_meta += "usedTextureMemorySize / Count</b>" + "\n";
                GUILayout.Label(us_meta);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                string us_metavalue = UnityStats.setPassCalls.ToString() + "\n";
                float rtBytes = UnityStats.renderTextureBytes / 1024f / 1024f;
                us_metavalue += rtBytes.ToString("F2")+" mb / "+UnityStats.renderTextureChanges+" / "+UnityStats.renderTextureCount + "\n";
                us_metavalue += UnityStats.usedTextureMemorySize+" / "+UnityStats.usedTextureCount;
                GUILayout.Label(us_metavalue);
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            #endif
            */

            //ProfilerRecorder - Rendering Module ============================
            string us_title = "<b>Profiler - Rendering Module Stats:</b>";
            GUILayout.Label(us_title);
            if(frameCount >= kAverageFrameCount -1)
            {
                us_stat_names = "";
                us_stat_values = "";
                for(int i=0; i<profilerRenderingRec.Length;i++)
                {
                    us_stat_names += profilerRenderingStat[i] + "\n";
                    if(profilerRenderingStat[i].Contains("Bytes"))
                    {
                        us_stat_values += BytesString(profilerRenderingRec[i].LastValue) + "\n";
                    }
                    else
                    {
                        us_stat_values += profilerRenderingRec[i].LastValue.ToString() + "\n";
                    }
                }
            }
            GUILayoutOption profilerRecorderStatStyle = GUILayout.Width(w*0.3f);
            GUILayout.BeginHorizontal();
            GUILayout.Label(us_stat_names,profilerRecorderStatStyle);
            GUILayout.Label(us_stat_values);
            GUILayout.EndHorizontal();

			//Memory =========================================================
            MemoryListUpdate();
            DataPairDisplay(memoryList);
            if(showTextureMemory)
            {
                TexMemoryListUpdate();
                DataPairDisplay(texmemoryList);
            }            
        }

        GUILayout.EndArea();
    }

    //================================TimeData
    #region TimeData

    internal class TimeData
    {
        public string name;

        //Starting frame and loading time
        public bool recordedLoadingData;
        public int startingFrame;
        public float loadingTime;
        
        //CPU timing
        public double realtimeAccTime;
        public double realtimeAvgTime;
        public double sampleAccTime;
        public double sampleAvgTime;
        
        //GPU timing
        public double realtimeAccTimeGPU;
        public double realtimeAvgTimeGPU;
        public double sampleAccTimeGPU;
        public double sampleAvgTimeGPU;

        public Recorder recorder;
    };

    private TimeData[] timeDataList;
    
    //Recorder, some pre-assigned profiler markers
    [Header("Profiler Marker Names")]
    public string[] profilerMarkerNames = new string[]
    {
        //SRP batcher & BRG relevant
        "SRPBRender.ApplyShader",
        "SRPBShadow.ApplyShader",
        "RenderLoop.DrawSRPBatcher",
        "Shadows.DrawSRPBatcher",
        "DrawBuffersBatchMode",
        //"SRPBatcher.DrawCommandsInjection" //This is BatchRendererGroup related

        //For detecting GPU-bound
        "Gfx.WaitForPresentOnGfxThread",
        "Semaphore.WaitForSignal", // see https://docs.unity3d.com/2022.2/Documentation/Manual/profiler-markers.html
        "WaitForTargetFPS" // mobile enforces vsync even vsync is off        
    };
    private bool gpuRecorder = true;
    
    //FrameTimings API
    ProfilerRecorder mainThreadTimeRecorder;
    ProfilerRecorder renderThreadTimeRecorder;
    private FrameTiming[] frametimings;
    private double GetAvgFrameTiming(uint count, int type)
    {
        double result = 0;
        if(count > 0)
        {
            for(int i=0; i<count; i++)
            {
                switch (type)
                {
                    case 0: result += frametimings[i].cpuFrameTime; break;
                    case 1: result += frametimings[i].gpuFrameTime; break;
                    //frametimings[i].cpuTimeFrameComplete + "\n";
                    //frametimings[i].cpuTimePresentCalled + "\n";
                    //frametimings[i].syncInterval + "\n";
                    //frametimings[i].widthScale + " x "+ frametimings[i].heightScale + "\n";
                }
            }
            result /= count;
        }
        return result;
    }

    private void TimeDataSetUp()
    {
        gpuRecorder = SystemInfo.supportsGpuRecorder;

        int totalFrameTimeDataCount = 4;
        timeDataList = new TimeData[profilerMarkerNames.Length + totalFrameTimeDataCount];

        for (int i=0;i<timeDataList.Length;i++)
        {
            timeDataList[i] = new TimeData();

            if (i==0)
            {
                //FrameTiming
                frametimings = new FrameTiming[10];
                timeDataList[i].name = "FrameTimings";
            }
            else if (i==1)
            {
                //FrameTiming Render Thread
                timeDataList[i].name = "CPU MainThread";
            }
            else if (i==2)
            {
                //FrameTiming Render Thread
                timeDataList[i].name = "CPU RenderThread";
            }
            else if (i==3)
            {
                //Render Pipeline Begin End Frame
                timeDataList[i].name = "RenderPipelineBeginEndFrame";
            }
            else
            {
                //Recorder - Profiler
                string markerName = profilerMarkerNames[i-totalFrameTimeDataCount];
                timeDataList[i].name = markerName;
                timeDataList[i].recorder = Recorder.Get(markerName);
                timeDataList[i].recorder.enabled = true;
                if(timeDataList[i].recorder == null)
                {
                    Debug.Log("Recorder is null: "+markerName);
                }
            }

            //Reset
            timeDataList[i].loadingTime = 0f;
            timeDataList[i].startingFrame = 0;
            timeDataList[i].recordedLoadingData = false;
            timeDataList[i].sampleAccTime = 0;
            timeDataList[i].sampleAvgTime = 0;
            timeDataList[i].sampleAccTimeGPU = 0;
            timeDataList[i].sampleAvgTimeGPU = 0;            
        }
    }

    private void TimeDataUpdateRealtime()
    {
        for (int i=0;i<timeDataList.Length;i++)
        {
            if(i==0)
            {
                //FrameTimings
                FrameTimingManager.CaptureFrameTimings();
                uint frametimingsCount = FrameTimingManager.GetLatestTimings((uint)frametimings.Length,frametimings);
                timeDataList[i].realtimeAccTime += GetAvgFrameTiming(frametimingsCount,0);
                timeDataList[i].realtimeAccTimeGPU += GetAvgFrameTiming(frametimingsCount,1);
            }
            else if(i==1)
            {
                //FrameTiming Main Thread
                timeDataList[i].realtimeAccTime += mainThreadTimeRecorder.LastValue;
            }
            else if(i==2)
            {
                //FrameTiming Render Thread
                timeDataList[i].realtimeAccTime += renderThreadTimeRecorder.LastValue;
            }
            else if(i==3)
            {
                //Render Pipeline Begin End Frame
                timeDataList[i].realtimeAccTime += renderPipelineBeginEndTime;
            }
            else
            {
                //Recorder
                timeDataList[i].realtimeAccTime += timeDataList[i].recorder.elapsedNanoseconds / 1000000.0f;
                timeDataList[i].realtimeAccTimeGPU += timeDataList[i].recorder.gpuElapsedNanoseconds / 1000000.0f;

                //When does the profiler marker start to have numbers = loading time
                if( !timeDataList[i].recordedLoadingData && timeDataList[i].realtimeAccTime > 0 )
                {
                    timeDataList[i].loadingTime = accDeltaTime;
                    timeDataList[i].startingFrame = accFrameCounter;
                    timeDataList[i].recordedLoadingData = true;
                }
            }
        }
    }

    private void TimeDataDoneRealtime()
    {
        for (int i=0;i<timeDataList.Length;i++)
        {
            //CPU
            timeDataList[i].realtimeAvgTime = timeDataList[i].realtimeAccTime / kAverageFrameCount;
            timeDataList[i].realtimeAccTime = 0.0f;
            
            //GPU
            timeDataList[i].realtimeAvgTimeGPU = timeDataList[i].realtimeAccTimeGPU / kAverageFrameCount;
            timeDataList[i].realtimeAccTimeGPU = 0.0f;

            //Frametiming main and render thread unit fix
            if(i==1) timeDataList[i].realtimeAvgTime  /= 1000000.0f;
            else if(i==2) timeDataList[i].realtimeAvgTime  /= 1000000.0f;
        }
    }

    private void TimeDataUpdateSample()
    {
        for (int i=0;i<timeDataList.Length;i++)
        {
            if(i==0)
            {
                //FrameTiming
                FrameTimingManager.CaptureFrameTimings();
                uint frametimingsCount = FrameTimingManager.GetLatestTimings((uint)frametimings.Length,frametimings);
                timeDataList[i].sampleAccTime += GetAvgFrameTiming(frametimingsCount,0);
                timeDataList[i].sampleAccTimeGPU += GetAvgFrameTiming(frametimingsCount,1);
            }
            else if(i==1)
            {
                //FrameTiming Main Thread
                timeDataList[i].sampleAccTime += mainThreadTimeRecorder.LastValue;
            }
            else if(i==2)
            {
                //FrameTiming Render Thread
                timeDataList[i].sampleAccTime += renderThreadTimeRecorder.LastValue;
            }
            else if(i==3)
            {
                //Render Pipeline Begin End Frame
                timeDataList[i].sampleAccTime += renderPipelineBeginEndTime;
            }
            else
            {
                //Recorder
                timeDataList[i].sampleAccTime += timeDataList[i].recorder.elapsedNanoseconds / 1000000.0f;
                timeDataList[i].sampleAccTimeGPU += timeDataList[i].recorder.gpuElapsedNanoseconds / 1000000.0f;
            }
        }
    }

    private void TimeDataDoneSample()
    {
        for (int i=0;i<timeDataList.Length;i++)
        {
            timeDataList[i].sampleAvgTime = timeDataList[i].sampleAccTime / sampleDataCount;
            
            timeDataList[i].sampleAvgTimeGPU = timeDataList[i].sampleAccTimeGPU / sampleDataCount;

            //Frametiming main and render thread unit fix
            if(i==1) timeDataList[i].sampleAvgTime  /= 1000000.0f;
            else if(i==2) timeDataList[i].sampleAvgTime /= 1000000.0f;
        }
    }

    //Replace zeros for better looking
    private string TimeDataFormatString(double time, string unit)
    {
        string result = "";
        if(time == 0)
        {
            result = "-";
        }
        else
        {
            result = System.String.Format("{0:F2}"+unit, time);
        }
        return result;
    }

    private void TimeDataDisplay()
    {
        string sName = "<b>Name</b>\n";
        string sLoad = "<b>Start Time | Frame</b>\n";
        string sTime = "<b>CPU (ms)</b>\n";
        string sTimeGPU = "<b>GPU (ms)</b>\n";
        string sSampledTime = "<b>CPU Sampled (ms)</b>\n";
        string sSampledTimeGPU = "<b>GPU Sampled (ms)</b>\n";

        for (int i = 0; i < timeDataList.Length; i++)
        {
            //Styles
            string colh = ""; //"<color=#ff0>";
            string colt = ""; //"</color>";

            //FPS
            string fpsName = "";
            string fpsValueRealtimeString = "";
            string fpsValueSampledTimeString = "";
            if(i == 0)
            {
                fpsName = " | FPS";
                double fpsValueRealtime = 1000f/timeDataList[i].realtimeAvgTime;
                double fpsValueSampledTime = 1000f/timeDataList[i].sampleAvgTime;
                fpsValueRealtimeString = " | " + Mathf.RoundToInt((float)fpsValueRealtime).ToString();
                fpsValueSampledTimeString = " | " + Mathf.RoundToInt((float)fpsValueSampledTime).ToString();
            }

            sName       += timeDataList[i].name + fpsName +"\n";
            sLoad       += colh + TimeDataFormatString(timeDataList[i].loadingTime,"s") + " | " + colt;
            sLoad      += colh + timeDataList[i].startingFrame + colt + "\n";
            sTime       += colh + TimeDataFormatString(timeDataList[i].realtimeAvgTime,"") + fpsValueRealtimeString + "\n" + colt;
            sTimeGPU       += colh + TimeDataFormatString(timeDataList[i].realtimeAvgTimeGPU,"") + "\n" + colt;
            sSampledTime    += colh + TimeDataFormatString(timeDataList[i].sampleAvgTime,"") + fpsValueSampledTimeString + "\n" + colt;
            sSampledTimeGPU    += colh + TimeDataFormatString(timeDataList[i].sampleAvgTimeGPU,"") + "\n" + colt;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(sName);
        if(showMarkerStartTiming) GUILayout.Label(sLoad);
        GUILayout.Label(CyanText(sTime));
        GUILayout.Label(CyanText(sTimeGPU));
        if(sampleStatus == 2) 
        {
            GUILayout.Label(GreenText(sSampledTime));
            GUILayout.Label(GreenText(sSampledTimeGPU));
        }
        else
        {
            GUILayout.Label(GreyText(sSampledTime));
            GUILayout.Label(GreyText(sSampledTimeGPU));
        }
        GUILayout.EndHorizontal();
    }

    #endregion

    //================================General Information
    #region GeneralInfo

    private DataPair[] generalInfoList;

    private void GeneralInfoListSetup()
    {
        generalInfoList = new DataPair[]
        {
            new DataPair("Scene name:",""),
            new DataPair("Screen resolution:",""),
            new DataPair("VSyncCount:",""),
            new DataPair("TargetFrameRate:",""),
            new DataPair("Unity:",""),
            new DataPair("Device:",""),
            new DataPair("OS:",""),
            new DataPair("CPU:",""),
            new DataPair("GPU:",""),
            new DataPair("RenderPipeline:",""),
            new DataPair("ColorSpace:",""),
            new DataPair("RenderingThreadMode:",""),
            new DataPair("Platform / API:",""),
        };
    }
    
    private void GeneralInfoListUpdate()
    {
        int i = 0;
        generalInfoList[i].content = currentSceneName; i++;
        generalInfoList[i].content = Screen.width+" x "+Screen.height; i++;
        generalInfoList[i].content = QualitySettings.vSyncCount + ""; i++;
        generalInfoList[i].content = Application.targetFrameRate + ""; i++;
        generalInfoList[i].content = Application.unityVersion + DetailedVersion.Get(); i++;
        generalInfoList[i].content = SystemInfo.deviceModel; i++;
        generalInfoList[i].content = SystemInfo.operatingSystem; i++;
        generalInfoList[i].content = SystemInfo.processorType; i++;
        generalInfoList[i].content = SystemInfo.graphicsDeviceName; i++;
        if(RenderPipelineManager.currentPipeline == null)
        {
            generalInfoList[i].content = "Built-in RenderPipeline"; i++;
        }
        else
        {
            generalInfoList[i].content = RenderPipelineManager.currentPipeline.ToString(); i++;
        }
        generalInfoList[i].content = QualitySettings.activeColorSpace + ""; i++;
        generalInfoList[i].content = SystemInfo.renderingThreadingMode.ToString(); i++;
        generalInfoList[i].content = Application.platform.ToString() + " / "+ SystemInfo.graphicsDeviceType; i++;
    }

    #endregion

    //================================Memory
    #region Memory

    private DataPair[] memoryList;
    private ProfilerRecorder systemMemoryRecorder; 

    private void MemoryListSetup()
    {
        //https://docs.unity3d.com/2021.2/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        memoryList = new DataPair[]
        {
            new DataPair("Allocated Mem For GfxDriver",""),
            new DataPair("Total Allocated Mem",""),
            new DataPair("Total Reserved Mem",""),
            new DataPair("System Used Mem",""),
            //new DataPair("Total Unused Reserved Mem",""),
            //new DataPair("Temp Allocator Size","")
        };
    }
    private void MemoryListUpdate()
    {
        memoryList[0].content = BytesString(UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver());
        memoryList[1].content = BytesString(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong());
        memoryList[2].content = BytesString(UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong());
        memoryList[3].content = BytesString(systemMemoryRecorder.LastValue);
        //memoryList[3].content = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024 + unit;
        //memoryList[4].content = UnityEngine.Profiling.Profiler.GetTempAllocatorSize() / 1024 / 1024 + unit;
    }

    #endregion
  
    //================================TextureMemory
    #region TextureMemory

    [Header("For Texture Memory")]
    public bool showTextureMemory = false;

    private DataPair[] texmemoryList;

    private void TexMemoryListSetup()
    {
        texmemoryList = new DataPair[]
        {
            new DataPair("Current Texture Memory",""),
            new DataPair("Desired Texture Memory (mem before streaming budget)",""),
            new DataPair("Non-Streaming Texture Memory (Texture Count)",""),
            new DataPair("Streaming Texture count",""),
            new DataPair("Total Texture Memory (mip0)",""),
            new DataPair("Target Texture Memory (mem after mip streaming)",""),
            new DataPair("StreamingRendererCount",""),
            new DataPair("StreamingMipmapUploadCount","")
        };
    }
    private void TexMemoryListUpdate()
    {
        texmemoryList[0].content = BytesString((long)Texture.currentTextureMemory);
        texmemoryList[1].content = BytesString((long)Texture.desiredTextureMemory);
        texmemoryList[2].content = BytesString((long)Texture.nonStreamingTextureMemory) + "(" + Texture.nonStreamingTextureCount + ")";
        texmemoryList[3].content = Texture.streamingTextureCount + "";
        texmemoryList[4].content = BytesString((long)Texture.totalTextureMemory);
        texmemoryList[5].content = BytesString((long)Texture.targetTextureMemory);
        texmemoryList[6].content = Texture.streamingRendererCount + "";
        texmemoryList[7].content = Texture.streamingMipmapUploadCount + "";
    }

    #endregion

    //================================End of Sample actions
    #region EndOfSampleActions

    [Header("End of Sample actions")]
    public bool doScreenCapture = false;
    public bool closeAppAfterScreenCap = false;
    private bool captured = false;

    public void EndofSampleActions()
    {
        //Screen capture
       // if ( Input.GetKeyDown(KeyCode.F9))
       // {
            //Scene scene = SceneManager.GetActiveScene();
            if(doScreenCapture)
            {
                string path = "screenshot_"+currentSceneName+"_"+SystemInfo.graphicsDeviceType+"_"+System.DateTime.Now.ToString("dd-mm-yyyy-hh-mm-ss")+".PNG";
                ScreenCapture.CaptureScreenshot(path);
            }
        //}
        captured = true;

        if(closeAppAfterScreenCap) Application.Quit();
    }

    #endregion

    //================================Scene Management
    #region SceneManagement

    public void NextScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex < SceneManager.sceneCountInBuildSettings - 1)
            SceneManager.LoadScene(sceneIndex + 1);
        else
            SceneManager.LoadScene(0);
    }

    public void PrevScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex > 0)
            SceneManager.LoadScene(sceneIndex - 1);
        else
            SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
    }

    #endregion

    //================================Toggles
    #region Toggles

    private void ToggleUI()
    {
        if(objUI == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if(canvas != null) objUI = canvas.gameObject;
        }
        else
        {
            objUI.SetActive( objUI.activeSelf ? false : true );
        }
    }

    private void ToggleMiniProfiler()
    {
        m_Enable = m_Enable ? false : true;
    }

    #endregion
    
    //================================For GUI
    #region ForGUI

    [Header("For GUI display")]
    [Range(0f,1f)] public float bgAlpha = 0.8f;
    public enum GuiAlign
    {
        Left,
        Middle,
        Right,
    };
    public GuiAlign guiAlign = GuiAlign.Middle;
    public int guiWidth = 1000;
    public int guiFontSize = 16;

    private GUIStyle labelRightAlignStyle;

	private Texture2D bktex;
    private void MakeBgTex()
    {
        int w = 2;
        int h = 2;
        Color col = new Color(0,0,0,1);
        Color[] pix = new Color[w * h];
        for( int i = 0; i < pix.Length; ++i )
        {
            pix[ i ] = col;
        }
        bktex = new Texture2D( w, h );
        bktex.SetPixels( pix );
        bktex.Apply();
    }

    private void ResetGUIBgColor()
    {
        //Need to hack the gamma <-> linear like this
        float a = bgAlpha; // the color i want is (1,1,1,0.8)
        Color col = new Color(a,a,a,1f);
        col = QualitySettings.activeColorSpace == ColorSpace.Linear? col.gamma : col;
        col = QualitySettings.activeColorSpace == ColorSpace.Linear? col.gamma : col;
        col.a = col.r;
        col.r = 1;
        col.g = 1;
        col.b = 1;
        GUI.backgroundColor = col;
    }

    struct DataPair
    {
        public string name;
        public string content;
        public DataPair(string n, string c)
        {
            name = n;
            content = c;
        }
    }

    private void DataPairDisplay(DataPair[] dataList)
    {
        //Contents
        string titles = "";
        string numbers = "";
        for (int i=0; i<dataList.Length;i++)
        {
            titles += dataList[i].name+"\n";
            numbers += dataList[i].content+"\n";
        }

        //Display
        GUILayout.BeginHorizontal();
        GUILayout.Label(titles);
        GUILayout.Label(numbers);
        GUILayout.EndHorizontal();
    }

    #endregion

    //================================TextStyles
    #region TextStyles

    private string DarkCyanText(string text)
    {
        return "<color=#099>" + text + "</color>";
    }
    private string CyanText(string text)
    {
        return "<color=#0ff>" + text + "</color>";
    }

    private string YellowText(string text)
    {
        return "<color=#ff0>" + text + "</color>";
    }

    private string RedText(string text)
    {
        return "<color=#f09>" + text + "</color>";
    }

    private string GreenText(string text)
    {
        return "<color=#0f0>" + text + "</color>";
    }

    private string GreyText(string text)
    {
        return "<color=#999>" + text + "</color>";
    }

    private string BooleanText(bool b)
    {
        if (b)
        {
            return "<color=#0f0>" + b.ToString() + "</color>";
        }
        else
        {
            return "<color=#f00>" + b.ToString() + "</color>";
        }
    }

    #endregion

    //================================Helpers
    string BytesString(long bytes)
    {
        float result = bytes;
        string unit = " bytes";
        if(result > 1024f)
        {
            result = result / 1024f;
            unit = " KB";
            
            if(result > 1024f)
            {
                result = result / 1024f;
                unit = " MB";

                if(result > 1024f)
                {
                    result = result / 1024f;
                    unit = " GB";
                }
            }
        }

        return String.Format("{0:0.00}", result)+unit;
    }
}

public class DetailedVersion
{
    public static string fileName = "detailedVersion";
    private static string st = "";

    public static string Get()
    {
        if(st=="")
        {
            #if UNITY_EDITOR
                int t = InternalEditorUtility.GetUnityVersionDate();
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                //string version_date = ""+dt.AddSeconds(t);
                string version_changeset = Convert.ToString(InternalEditorUtility.GetUnityRevision(), 16);
                string version_branch = InternalEditorUtility.GetUnityBuildBranch();
                st = "("+version_changeset+") "+version_branch;//+" "+version_date;
            #else
                var textFile = Resources.Load<TextAsset>(fileName);
                st = textFile.text;
            #endif
        }
        return st;
    }
}

#if UNITY_EDITOR
class DetailedVersionFile : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return -100; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        //Make sure Resources folder exists
        if (!AssetDatabase.IsValidFolder("Assets\\Resources"))
        {
            //Debug.Log("folder not exist");
            string guid = AssetDatabase.CreateFolder ("Assets", "Resources");
            AssetDatabase.Refresh();
        }

        //Write version text file
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/"+DetailedVersion.fileName+".txt", DetailedVersion.Get());
        AssetDatabase.Refresh();
    }
}
#endif