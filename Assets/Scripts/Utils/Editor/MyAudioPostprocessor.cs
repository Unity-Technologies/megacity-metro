using UnityEditor;
using UnityEngine;

namespace Unity.MegacityMetro.EditorTools
{
    class MyAudioPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            var audioImporter = assetImporter as AudioImporter;
            var settings = new AudioImporterSampleSettings();
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.quality = 0.7f;
            audioImporter.SetOverrideSampleSettings(BuildTargetGroup.Android, settings);
        }
    }
}