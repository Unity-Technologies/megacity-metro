using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace Unity.NAME.Tests
{
    class EditorExampleTest
    {
        [Test]
        public void EditorSampleTestSimplePasses()
        {
            // Use the Assert class to test conditions.
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator EditorSampleTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }
    }
}
