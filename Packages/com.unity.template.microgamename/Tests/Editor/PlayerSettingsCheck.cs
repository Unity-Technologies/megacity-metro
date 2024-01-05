using NUnit.Framework;
using System;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.NAME.Tests
{
    class PlayerSettingsCheck
    {
        const string k_ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        UnityEngine.Object m_ProjectSettings;

        [Serializable]
        class PkgInfo
        {
            public string name = "";
            public string displayName = "";
            public string version = "";
            public string type = "";
            public string unity = "";
            public string description = "";
            //public object dependencies;
        }

        [SetUp]
        public void SetUp()
        {
            m_ProjectSettings = InternalEditorUtility.LoadSerializedFileAndForget(k_ProjectSettingsPath)[0];
        }

        [Test]
        public void PlayerSettingsMatchPackageInfo()
        {
            var packagePath = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly()).assetPath;
            var packageInfoPath = $"{packagePath}/package.json";

            var packageInfo = JsonUtility.FromJson<PkgInfo>(File.ReadAllText(packageInfoPath));

            // TODO upm-ci doesn't support running tests for WebGL or mobile builds yet so this is not run in
            // the correct environment using CI.
            // NOTE Template Isolation Test overwrites Application.identifier with "com.Company.ProductName"

            using (var so = new SerializedObject(m_ProjectSettings))
            {
                Assert.AreEqual(
                    so.FindProperty("applicationIdentifier.Array.data[0].second")?.stringValue, packageInfo.name,
                    "ProjectSettings' applicationIdentifier doesn't have the package name set."
                );

                Assert.AreEqual(
                    packageInfo.version, Application.version,
                    "PackageInfo.version and Application.version do not match."
                );
            }
        }

        [Test]
        public void TemplateDefaultSceneIsEmpty()
        {
            using (var so = new SerializedObject(m_ProjectSettings))
            {
                Assert.True(
                    string.IsNullOrEmpty(so.FindProperty("templateDefaultScene").stringValue),
                    $"'templateDefaultScene' in {k_ProjectSettingsPath} not empty."
                );
            }
        }
    }
}
