using System;
using System.Linq;
using Unity.Services.Core.Editor.Environments;
using UnityEngine;
using UnityEditor;
using Unity.Tutorials.Core.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

/// <summary>
/// Implement your Tutorial callbacks here.
/// </summary>
[CreateAssetMenu(fileName = DefaultFileName, menuName = "Tutorials/" + DefaultFileName + " Instance")]
public class TutorialCallbacks : ScriptableObject
{
    /// <summary>
    /// The default file name used to create asset of this class type.
    /// </summary>
    public const string DefaultFileName = "TutorialCallbacks";
    
    /// <summary>
    /// Creates a TutorialCallbacks asset and shows it in the Project window.
    /// </summary>
    /// <param name="assetPath">
    /// A relative path to the project's root. If not provided, the Project window's currently active folder path is used.
    /// </param>
    /// <returns>The created asset</returns>
    public static ScriptableObject CreateAndShowAsset(string assetPath = null)
    {
    
        assetPath = assetPath ?? $"{TutorialEditorUtils.GetActiveFolderPath()}/{DefaultFileName}.asset";
        var asset = CreateInstance<TutorialCallbacks>();
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
        EditorUtility.FocusProjectWindow(); // needed in order to make the selection of newly created asset to really work
        Selection.activeObject = asset;
        return asset;
    }

    public void StartTutorial()
    {
        TutorialWindow.ShowWindow();
    }

    // Project Link
    public bool IsProjectLinked()
    {
        return CloudProjectSettings.projectBound;
    }

    // Environments
    double m_EnvironmentSetLastTime;
    string m_EnvironmentName;

    public bool IsEnvironmentSet()
    {
        // ActiveEnvironmentName is IO heavy, so we'll throttle it
        if (EditorApplication.timeSinceStartup - m_EnvironmentSetLastTime > 0.5)
        {
            m_EnvironmentSetLastTime = EditorApplication.timeSinceStartup;
            m_EnvironmentName = EnvironmentsApi.Instance.ActiveEnvironmentName;
        }

        return !string.IsNullOrEmpty(m_EnvironmentName);
    }
    
    double m_LinuxPoll;
    public bool LinuxDedicatedServerEnv()
    {
        if (EditorApplication.timeSinceStartup - m_LinuxPoll > 0.5)
        {
            m_LinuxPoll = EditorApplication.timeSinceStartup;
        }
        
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64
            && EditorUserBuildSettings.standaloneBuildSubtarget == StandaloneBuildSubtarget.Server)
            return true;

        return false;
    }

    // Deployment
    bool m_IsAssetsDeployed;

    Type m_DeploymentWindowType;
    ToolbarButton m_DeploySelectedButton;

    public void ShowDeploymentWindowCompleted()
    {
        m_IsAssetsDeployed = false;

        if (m_DeploymentWindowType == null)
        {
            m_DeploymentWindowType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.FullName == "Unity.Services.Deployment.Editor.Interface.UI.DeploymentWindow");
            if (m_DeploymentWindowType == null)
            {
                return;
            }
        }

        var window = EditorWindow.GetWindow(m_DeploymentWindowType);
        m_DeploySelectedButton = window.rootVisualElement.Q<ToolbarButton>("DeploySelectedButton");
        if (m_DeploySelectedButton != null)
        {
            m_DeploySelectedButton.clicked += DeploySelectedButtonClicked;
        }
    }

    public void ShowDeploymentWindowInvalidated()
    {
        m_IsAssetsDeployed = false;
        if (m_DeploySelectedButton != null)
        {
            m_DeploySelectedButton.clicked -= DeploySelectedButtonClicked;
        }
    }

    void DeploySelectedButtonClicked()
    {
        m_IsAssetsDeployed = true;
    }

    public bool IsAssetsDeployed()
    {
        return m_IsAssetsDeployed;
    }
}
