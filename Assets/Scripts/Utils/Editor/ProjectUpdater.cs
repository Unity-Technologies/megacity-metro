using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

public class ProjectUpdater : EditorWindow
{
    private string projectSourceFolder;
    private string projectDestinationFolder;

    [MenuItem("Tools/Project Updater")]
    public static void ShowWindow()
    {
        GetWindow<ProjectUpdater>("Project Updater");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Update Packages"))
        {
            UpdatePackageVersionsFromFile();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        projectSourceFolder = Directory.GetParent(Application.dataPath).FullName;

        GUILayout.Label("Project Destination Folder", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        projectDestinationFolder = EditorGUILayout.TextField(projectDestinationFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            projectDestinationFolder = EditorUtility.OpenFolderPanel("Select Project Source Folder", "", "");
            GUI.FocusControl(null); // Remove focus from text field to refresh GUI
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(projectDestinationFolder));
        if (GUILayout.Button("Migrate"))
        {
            UpdateProject();
            // Show a notification dialog when the update is completed
            EditorUtility.DisplayDialog("Update Complete", "The project destination has been updated.", "OK");
        }
        EditorGUI.EndDisabledGroup();
        
        GUILayout.Space(10);
    }

    private void UpdateProject()
    {
        // Delete existing Assets, Packages, and Project Settings folders in the destination folder
        DeleteContentFolder(projectDestinationFolder);
    
        // Copy Assets, Packages, and Project Settings folders from source to destination
        CopyFolder(Path.Combine(projectSourceFolder, "Assets"), Path.Combine(projectDestinationFolder, "Assets"));
        CopyFolder(Path.Combine(projectSourceFolder, "Packages"), Path.Combine(projectDestinationFolder, "Packages"));
        CopyFolder(Path.Combine(projectSourceFolder, "ProjectSettings"),
            Path.Combine(projectDestinationFolder, "ProjectSettings"));
        CopyFolder(Path.Combine(projectSourceFolder, "../Readme"), Path.Combine(projectDestinationFolder, "Readme"));
        CopyFolder(Path.Combine(projectSourceFolder, "../Documentation"), Path.Combine(projectDestinationFolder, "Documentation"));
        CopyFolder(Path.Combine(projectSourceFolder, "NativePlugins"), Path.Combine(projectDestinationFolder, "NativePlugins"));
        
        // Copy LICENCE.md, README.md, and Third Party Notices.md from parent folder to the destination folder
        var parentFolder = Directory.GetParent(projectSourceFolder).FullName;
        CopyFile(Path.Combine(parentFolder, "LICENCE.md"), Path.Combine(projectDestinationFolder, "LICENCE.md"));
        CopyFile(Path.Combine(parentFolder, "README.md"), Path.Combine(projectDestinationFolder, "README.md"));
        
        // Update README.md file in the destination folder
        var readmeFilePath = Path.Combine(projectDestinationFolder, "README.md");
        UpdateReadmeFile(readmeFilePath);
        
        // Update ProjectSettings.asset to avoid sharing the internal dots-ugs-credentials
        var projectSettingsFilePath = Path.Combine(projectDestinationFolder, "ProjectSettings", "ProjectSettings.asset");
        UpdateProjectSettings(projectSettingsFilePath);
        
        var documentationFolder = Path.Combine(projectDestinationFolder, "Documentation");
        UpdateDocumentation(documentationFolder);
    }

    private void DeleteContentFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            var files = Directory.GetFiles(folderPath);
            var paths = Directory.GetDirectories(folderPath);

            foreach (string file in files)
            {
                if(file.Contains("gitattributes") ||
                   file.Contains("gitignore"))
                    continue;
                
                File.Delete(file);
            }

            foreach (string dir in paths)
            {
                if (folderPath.Contains("git"))
                    continue;
                
                Directory.Delete(dir, true);
            }
        }
    }

    private void CopyFolder(string sourceFolderPath, string destinationFolderPath)
    {
        if (Directory.Exists(sourceFolderPath))
        {
            Directory.CreateDirectory(destinationFolderPath);

            foreach (string filePath in Directory.GetFiles(sourceFolderPath))
            {
                //Avoid copying Editor internal tools
                if (sourceFolderPath.Contains("Utils") && filePath.Contains("Editor"))
                    continue;

                if(filePath.Contains("GfxProfiler") || filePath.Contains("GFXProfilingTool"))
                    continue;
                
                string fileName = Path.GetFileName(filePath);
                string destinationPath = Path.Combine(destinationFolderPath, fileName);
                File.Copy(filePath, destinationPath, true);
            }

            foreach (string subFolderPath in Directory.GetDirectories(sourceFolderPath))
            {
                //Avoid copying Editor internal tools
                if (sourceFolderPath.Contains("Utils") && subFolderPath.Contains("Editor"))
                    continue;

                if(subFolderPath.Contains("GFXProfilingTool"))
                    continue;
                    
                string subFolderName = Path.GetFileName(subFolderPath);
                string destinationSubFolderPath = Path.Combine(destinationFolderPath, subFolderName);
                CopyFolder(subFolderPath, destinationSubFolderPath);
            }
        }
    }

    private void CopyFile(string sourceFilePath, string destinationFilePath)
    {
        if (File.Exists(sourceFilePath))
        {
            File.Copy(sourceFilePath, destinationFilePath, true);
        }
    }

    private void UpdateReadmeFile(string readmeFilePath)
    {
        if (File.Exists(readmeFilePath))
        {
            string[] lines = File.ReadAllLines(readmeFilePath);
            bool modified = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                
                if (line.Contains("https://github.cds.internal.unity3d.com/unity/"))
                {
                    line = line.Replace("https://github.cds.internal.unity3d.com/unity/", "https://github.com/Unity-Technologies/");
                    lines[i] = line;
                    modified = true;
                }
            }

            if (modified)
            {
                File.WriteAllLines(readmeFilePath, lines);
                Debug.Log("Readme file updated successfully.");
            }
        }
    }

    private void UpdateProjectSettings(string path)
    {
        if (File.Exists(path))
        {
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("cloudProjectId:"))
                {
                    lines[i] = "  cloudProjectId: ";
                }
                else if (lines[i].Contains("projectName:"))
                {
                    lines[i] = "  projectName: ";
                }
                else if (lines[i].Contains("organizationId:"))
                {
                    lines[i] = "  organizationId: ";
                }
                else if (lines[i].Contains("productGUID:"))
                {
                    lines[i] = "  productGUID: 98f9a8c815eba2a4c8fe254e1e57fb6b";
                }
            }

            File.WriteAllLines(path, lines);
        }
    }

    private void UpdateDocumentation(string path)
    {
        if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                UpdateReadmeLink(file);
            }
        }
    }

    private void UpdateReadmeLink(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        bool modified = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            
            if (line.Contains("Megacity-Metro/Assets"))
            {
                line = line.Replace("Megacity-Metro/Assets", "Assets");
                lines[i] = line;
                modified = true;
            }
        }

        if (modified)
        {
            File.WriteAllLines(filePath, lines);
        }   
    }

    private void UpdatePackageVersionsFromFile()
    {
        var path= Directory.GetParent(Application.dataPath).FullName;
        
        var filePath = Path.Combine(path, "Packages/manifest.json"); 
        var jsonContent = File.ReadAllText(filePath); 

        if (!string.IsNullOrEmpty(jsonContent))
        {
            var packageVersions = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonContent);
            var dependencies = packageVersions["dependencies"];
            var newDependencies = new Dictionary<string, string>();
            foreach (var package in dependencies)
            {
                var packageName = package.Key;
                var request = Client.List(true, false);
                while (!request.IsCompleted)
                {
                    
                }

                if (request.Status == StatusCode.Success)
                {
                    foreach (var installedPackage in request.Result)
                    {
                        if (installedPackage.name == packageName)
                        {
                            AddRequest addRequest = Client.Add(packageName);
                            while (!addRequest.IsCompleted)
                            {
                                
                            }

                            if (addRequest.Status == StatusCode.Success)
                            {
                                newDependencies.Add(packageName, addRequest.Result.versions.latest);
                            }
                            else
                            {
                                Debug.LogError($"Error when updating package {packageName}: {addRequest.Error.message}");
                            }

                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("List of installed packages couldn't be found: " + request.Error.message);
                }
            }

            packageVersions["dependencies"] = newDependencies;
            var updatedJson = JsonConvert.SerializeObject(packageVersions, Formatting.Indented);
            File.WriteAllText(filePath, updatedJson);
            Debug.Log($"packages updated {updatedJson}");
        }
        else
        {
            Debug.LogError($"File not found {filePath}");
        }
    }
}
