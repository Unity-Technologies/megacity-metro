using System;
using System.Collections;
using System.Collections.Generic;
using Unity.NetCode.Hybrid;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#if UNITY_STANDALONE_OSX
using UnityEditor.OSXStandalone;
#endif
public class BuilderScript : MonoBehaviour
{
    [MenuItem("Tools/Builder/Build Android Vulkan")]
    static void BuildAndroidVulkan()
    {
        //enforcing the il2cpp backend
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        //enforcing Vulkan
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new []{GraphicsDeviceType.Vulkan});
        PlayerSettings.SetArchitecture(BuildTargetGroup.Android,1);
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.Android;
        // SubTarget expects an integer.
        buildPlayerOptions.options = BuildOptions.Development | BuildOptions.ShowBuiltPlayer;
        buildPlayerOptions.locationPathName = "./build/Android_Vulkan/MegacityMetro.apk";
        buildPlayerOptions.extraScriptingDefines = new[] { "NETCODE_DEBUG", "UNITY_CLIENT" };
      
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    [MenuItem("Tools/Builder/Build Release Android Vulkan")]
    static void BuildReleaseAndroidVulkan()
    {
        //enforcing the il2cpp backend
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        //enforcing Vulkan
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new []{GraphicsDeviceType.Vulkan});
        PlayerSettings.SetArchitecture(BuildTargetGroup.Android,1);
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.Android;
        // SubTarget expects an integer.
        buildPlayerOptions.options =  BuildOptions.ShowBuiltPlayer;
        buildPlayerOptions.locationPathName = "./build/Android_Vulkan/MegacityMetro.apk";
        buildPlayerOptions.extraScriptingDefines = new[] { "NETCODE_DEBUG", "UNITY_CLIENT" };
      
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    
    [MenuItem("Tools/Builder/Build Android With Server Vulkan")]
    static void BuildAndroidVulkanWithServer()
    {
        //enforcing the il2cpp backend
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        //enforcing Vulkan
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new []{GraphicsDeviceType.Vulkan});
        PlayerSettings.SetArchitecture(BuildTargetGroup.Android,1);
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.Development;
        // SubTarget expects an integer.
        buildPlayerOptions.locationPathName = "./build/Android_Vulkan_WithServer/MegacityMetro.apk";
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    [MenuItem("Tools/Builder/Build Android GLES3")]
    static void BuildAndroidGLES3()
    {
        //enforcing the il2cpp backend
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        //enforcing GLES3
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new []{GraphicsDeviceType.OpenGLES3});
        PlayerSettings.SetArchitecture(BuildTargetGroup.Android,1);
        NetCodeClientSettings.instance.ClientTarget = NetCodeClientTarget.Client;
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.Android;
        // SubTarget expects an integer.
        buildPlayerOptions.options = BuildOptions.Development | BuildOptions.ShowBuiltPlayer;
        buildPlayerOptions.locationPathName = "./build/Android_GLES/MegacityMetro.apk";
        buildPlayerOptions.extraScriptingDefines = new[] { "NETCODE_DEBUG", "UNITY_CLIENT" };
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    
    [MenuItem("Tools/Builder/Build Dedicated Server Windows")]
    static void BuildDedicatedServerWindows()
    {
        //enforcing the il2cpp backend
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        AssetDatabase.SaveAssets();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        // SubTarget expects an integer.
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
        //this is needed for profiling 
        //buildPlayerOptions.options = BuildOptions.Development;
        buildPlayerOptions.locationPathName = "./build/Server-Win/Server.exe";

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    
    [MenuItem("Tools/Builder/Build Dedicated Server Linux")]
    static void BuildDedicatedServerLinux()
    {
        //enforcing the il2cpp backend
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Server, ScriptingImplementation.IL2CPP);
        AssetDatabase.SaveAssets();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        // SubTarget expects an integer.
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
        //this is needed for profiling 
        //buildPlayerOptions.options = BuildOptions.Development;
        buildPlayerOptions.locationPathName = "./build/Server/Server.x86_64";

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    
    [MenuItem("Tools/Builder/Build Standalone Windows Vulkan")]
    static void BuildStandAloneWindowsVulkan()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        NetCodeClientSettings.instance.ClientTarget = NetCodeClientTarget.Client;
        //enforcing Vulkan
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new []{GraphicsDeviceType.Vulkan});
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        //this is needed for profiling 
        buildPlayerOptions.options = BuildOptions.Development;
        buildPlayerOptions.locationPathName = "./build/MegacityMetro-Win-Client/MegacityMetro.exe";
        buildPlayerOptions.extraScriptingDefines = new[] { "NETCODE_DEBUG", "UNITY_CLIENT" };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    
    [MenuItem("Tools/Builder/Build Standalone Windows DX11")]
    static void BuildStandAloneWindowsDX11()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        NetCodeClientSettings.instance.ClientTarget = NetCodeClientTarget.Client;
        //enforcing Dx11
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new []{GraphicsDeviceType.Direct3D11});
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        //this is needed for profiling 
        buildPlayerOptions.options = BuildOptions.Development | BuildOptions.ShowBuiltPlayer;
        buildPlayerOptions.locationPathName = "./build/MegacityMetro-Win-Client/MegacityMetro.exe";
        buildPlayerOptions.extraScriptingDefines = new[] { "NETCODE_DEBUG", "UNITY_CLIENT" };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    [MenuItem("Tools/Builder/Build Release Standalone Windows DX11")]
    static void BuildReleaseStandAloneWindowsDX11()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        NetCodeClientSettings.instance.ClientTarget = NetCodeClientTarget.Client;
        //enforcing Dx11
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new []{GraphicsDeviceType.Direct3D11});
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        //this is needed for profiling 
        buildPlayerOptions.options =  BuildOptions.ShowBuiltPlayer;
        buildPlayerOptions.locationPathName = "./build/MegacityMetro-Win-Client/MegacityMetro.exe";
        buildPlayerOptions.extraScriptingDefines = new[] { "NETCODE_DEBUG", "UNITY_CLIENT" };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    
    [MenuItem("Tools/Builder/Build Standalone Windows DX12")]
    static void BuildStandAloneWindowsDX12()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        NetCodeClientSettings.instance.ClientTarget = NetCodeClientTarget.Client;
        //enforcing DX12
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new []{GraphicsDeviceType.Direct3D12});
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        //this is needed for profiling 
        buildPlayerOptions.options = BuildOptions.Development;
        buildPlayerOptions.locationPathName = "./build/MegacityMetro-Win-Client/MegacityMetro.exe";
        buildPlayerOptions.extraScriptingDefines = new[] { "NETCODE_DEBUG", "UNITY_CLIENT" };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
#if UNITY_STANDALONE_OSX
    private static void BuildStandAloneOSX(OSArchitecture macOSArch, 
        Il2CppCodeGeneration codeGeneration = Il2CppCodeGeneration.OptimizeSpeed,
        Il2CppCompilerConfiguration compilerConfiguration = Il2CppCompilerConfiguration.Master)
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Standalone, compilerConfiguration);
        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, codeGeneration);
        
        NetCodeClientSettings.instance.ClientTarget = NetCodeClientTarget.Client;
        //enforcing Metal Graphics API
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneOSX, new []{GraphicsDeviceType.Metal});
        AssetDatabase.SaveAssets();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Menu.unity","Assets/Scenes/Main.unity" };
        buildPlayerOptions.target = BuildTarget.StandaloneOSX;
        
        // Select MacOS build target
        UserBuildSettings.architecture = macOSArch;
        
        //this is needed for profiling 
        buildPlayerOptions.options = BuildOptions.Development;
        buildPlayerOptions.locationPathName = "./build/Megacity-Metro-MacOS-Client/Megacity" + Enum.GetName(typeof(OSArchitecture), UserBuildSettings.architecture) + ".app";
        buildPlayerOptions.extraScriptingDefines = new[] { "NETCODE_DEBUG", "UNITY_CLIENT" };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    
    [MenuItem("Tools/Builder/Build Standalone MacOS Universal")]
    static void BuildStandAloneOSX_Universal()
    {
        BuildStandAloneOSX(OSArchitecture.x64ARM64);
    }

    [MenuItem("Tools/Builder/Build Standalone MacOS Intelx64")]
    static void BuildStandAloneOSX_Intelx64()
    {
        BuildStandAloneOSX(OSArchitecture.x64);
    }
    
    [MenuItem("Tools/Builder/Build Standalone MacOS Silicon")]
    static void BuildStandAloneOSX_Silicon()
    {
        BuildStandAloneOSX(OSArchitecture.ARM64);
    }
#endif
}