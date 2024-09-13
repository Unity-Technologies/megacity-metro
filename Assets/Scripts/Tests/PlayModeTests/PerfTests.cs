using System.Collections;
using NUnit.Framework.Internal;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;


namespace Tests
{
    public class PerfTests
    {
        private const int testDurationFrames = 1000;
        private const int waitForSimulationWarmUPFrames = 1000;
        private const float testDurationTime = 33f;
        private const float waitForSimulationWarmUPTime = 33f;
        public string sceneName = "Main";

        [UnityTest, Performance]
        [Timeout(5000000)]
        public IEnumerator MainScenePerformance()
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
                yield return SceneManager.UnloadSceneAsync(sceneName);
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            SampleGroup[] m_definitions =
            {
                //for more markers refer to this list https://docs.unity3d.com/Manual/profiler-markers.html
                new SampleGroup("PlayerLoop"),
                //new SampleGroup("Gfx.PresentFrame"),
                //new SampleGroup("GC.Alloc"),
                //for multi threaded codes
                new SampleGroup("Idle"),
                new SampleGroup("JobHandle.Complete"),
            };

            //capture the performance of the main scene for the given frame count 
            yield return Measure.Frames().WarmupCount(waitForSimulationWarmUPFrames).MeasurementCount(testDurationFrames).ProfilerMarkers(m_definitions).Run();
            
            //capture the performance of the main scene for the given duration
            // yield return new WaitForSeconds(waitForSimulationWarmUPTime);
            // using (Measure.ProfilerMarkers(m_definitions))
            // {
            //     yield return new WaitForSeconds(testDurationTime);
            // }

            var allocated = new SampleGroup("TotalAllocatedMemory", SampleUnit.Megabyte);
            var reserved = new SampleGroup("TotalReservedMemory", SampleUnit.Megabyte);

            Measure.Custom(allocated, Profiler.GetTotalAllocatedMemoryLong() / 1048576f);
            Measure.Custom(reserved, Profiler.GetTotalReservedMemoryLong() / 1048576f);
            
            yield return null;
        }
        
    }
}
