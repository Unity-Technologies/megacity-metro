using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.NAME.Utilities
{
    [CreateAssetMenu(menuName = "Microgame/BuildSettings")]
    public class BuildSettings : ScriptableObject
    {
        public WebGlBuildSettings Settings = new WebGlBuildSettings();
        public string BuildName;
        public BuildType BuildType;
        public string ShaderType;
    }

    public enum BuildType
    {
        DevelopmentUncompressed,
        ReleaseGzipped,
        ReleaseUncompressed,
        Profiling,
        CpuProfiler,
        MemoryProfiler
    }

    [Serializable]
    public class WebGlBuildSettings
    {
        public bool DevelopmentBuild;
        public bool AutoconnectProfiler;
        public GraphicsDeviceType[] GraphicsDeviceSettings;
        public bool HalfResolution;

        public GraphicsDeviceType[] SetGraphicsDevices(bool webGl1, bool webGl2)
        {
            return webGl1 && webGl2
                ? new[] {GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLES2}
                : new[] {webGl1? GraphicsDeviceType.OpenGLES2 : GraphicsDeviceType.OpenGLES3};
        }

        public string GetName()
        {
            string name = "";
            for (int i = 0; i < GraphicsDeviceSettings.Length; i++)
            {
                name += "_" + GraphicsDeviceSettings[i].ToString();
            }

            if (AutoconnectProfiler) name += "_profiler";
            if (HalfResolution) name += "_halfRes";
            return name;
        }

        public WebGlBuildSettings(bool developmentBuild = false,
                                  bool autoconnectProfiler = false, bool halfResolution = false,  GraphicsDeviceType[] graphicsDeviceSettings = null)
        {
            this.GraphicsDeviceSettings = graphicsDeviceSettings ?? SetGraphicsDevices(true, true);
            this.DevelopmentBuild = developmentBuild;
            this.HalfResolution = halfResolution;
            this.AutoconnectProfiler = autoconnectProfiler;
        }
    }
}
