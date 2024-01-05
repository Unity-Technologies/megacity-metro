using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace Unity.NAME.Tests
{
    public class RuntimeExampleTest
    {
        [Test]
        public void RuntimeExampleTestSimplePasses()
        {
            // Use the Assert class to test conditions.
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator RuntimeExampleTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }
    }
}
