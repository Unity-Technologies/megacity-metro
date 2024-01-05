using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Unity.NAME.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unity.NAME.Tests
{
    public class BuildSettingsCheck
    {
        static string buildPath
        {
            get
            {
                // NOTE Use "Builds" subfolder in order to make this test pass locally when
                // using Windows & Visual Studio
                const string buildName = "Builds/BuildPlayerTests_Build";
                if (Application.platform == RuntimePlatform.OSXEditor)
                    return buildName + ".app";
                return buildName;
            }
        }

        [SetUp]
        public void SetUp()
        {
            Assert.That(File.Exists(buildPath), Is.False, "Existing file at path " + buildPath);
            Assert.That(Directory.Exists(buildPath), Is.False, "Existing directory at path " + buildPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(buildPath))
                File.Delete(buildPath);

            if (Directory.Exists(buildPath))
                Directory.Delete(buildPath, true);
        }

        static BuildTarget buildTarget
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                        return BuildTarget.StandaloneOSX;

                    case RuntimePlatform.WindowsEditor:
                        return BuildTarget.StandaloneWindows;

                    case RuntimePlatform.LinuxEditor:
                        // NOTE Universal & 32-bit Linux support dropped after 2018 LTS
                        return BuildTarget.StandaloneLinux64;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [Test]
        public void EnsureStandaloneBuild()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new string[EditorBuildSettings.scenes.Length],
                locationPathName = buildPath,
                targetGroup = BuildTargetGroup.Standalone,
                target = buildTarget,
                options = BuildOptions.StrictMode,
            };

            for (var sceneId = 0; sceneId < EditorBuildSettings.scenes.Length; sceneId++)
            {
                buildPlayerOptions.scenes[sceneId] = EditorBuildSettings.scenes[sceneId].path;
            }

            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }


        [Test]
        public void GameSceneManagerSetup()
        {
            // At least one scene
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Assert.GreaterOrEqual(scenes.Length, 1, "You need at least one scene in the Editor build settings");

            foreach (EditorBuildSettingsScene scene in scenes)
            {
                EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
            }

            var gameSceneManager = UnityEngine.Object.FindObjectsOfType<GameSceneManager>().FirstOrDefault();
            Assert.IsTrue(gameSceneManager != null, "No GameSceneManager found in any scene");

            if (gameSceneManager != null)
            {
                Assert.IsTrue(gameSceneManager.IntroScene != null, "The Intro Scene is missing in your GameSceneManager");
                Assert.IsTrue(gameSceneManager.FailureScene != null, "The Failure Scene is missing in your GameSceneManager");
                Assert.IsTrue(gameSceneManager.SuccessScene != null, "The Success Scene is missing in your GameSceneManager");
                Assert.IsTrue(gameSceneManager.MainLevels != null && gameSceneManager.MainLevels[0] != null, "There is no Game Levels in your GameSceneManager");
            }
        }
    }
}
