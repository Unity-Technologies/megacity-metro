using UnityEditor;

namespace Unity.MegacityMetro.EditorTools
{
    /// <summary>
    /// Helper function to automatically format the project textures according to the target platform
    /// </summary>
    class MyTexturePostprocessor : AssetPostprocessor
    {
        struct PlatformSettings
        {
            public string Name;
            public int Size;
            public TextureImporterFormat Format;

            public TextureImporterPlatformSettings GetPlatformSettings(TextureImporter importer)
            {
                var platformSettings = new TextureImporterPlatformSettings();
                importer.GetDefaultPlatformTextureSettings().CopyTo(platformSettings);
                platformSettings.overridden = true;
                platformSettings.name = Name;
                if (Size > 0)
                {
                    platformSettings.maxTextureSize = Size;
                }

                platformSettings.format = Format;
                return platformSettings;
            }
        }

        PlatformSettings[] albedoMap_PlatformSettings =
        {
            new PlatformSettings
            {
                Name = "Standalone",
                Format = TextureImporterFormat.BC7
            },
            new PlatformSettings
            {
                Name = "PS4",
                Format = TextureImporterFormat.BC7
            }
        };

        PlatformSettings[] maskMap_PlatformSettings =
        {
            new PlatformSettings
            {
                Name = "Standalone",
                Format = TextureImporterFormat.BC7
            },
            new PlatformSettings
            {
                Name = "PS4",
                Format = TextureImporterFormat.BC7
            }
        };

        PlatformSettings[] layerMask_PlatformSettings =
        {
            new PlatformSettings
            {
                Name = "Standalone",
                Format = TextureImporterFormat.BC7
            },
            new PlatformSettings
            {
                Name = "PS4",
                Format = TextureImporterFormat.BC7
            }
        };

        PlatformSettings[] normalMap_PlatformSettings =
        {
            new PlatformSettings
            {
                Name = "Standalone",
                Format = TextureImporterFormat.BC5
            },
            new PlatformSettings
            {
                Name = "PS4",
                Format = TextureImporterFormat.BC5
            }
        };

        PlatformSettings[] bentnormalMap_PlatformSettings =
        {
            new PlatformSettings
            {
                Name = "Standalone",
                Size = 1024,
                Format = TextureImporterFormat.BC5
            },
            new PlatformSettings
            {
                Name = "PS4",
                Size = 256,
                Format = TextureImporterFormat.BC5
            }
        };

        PlatformSettings[] heightMap_PlatformSettings =
        {
            new PlatformSettings
            {
                Name = "Standalone",
                Format = TextureImporterFormat.BC4
            },
            new PlatformSettings
            {
                Name = "PS4",
                Format = TextureImporterFormat.BC4
            }
        };

        PlatformSettings[] thicknessMap_PlatformSettings =
        {
            new PlatformSettings
            {
                Name = "Standalone",
                Format = TextureImporterFormat.BC4
            },
            new PlatformSettings
            {
                Name = "PS4",
                Format = TextureImporterFormat.BC4
            }
        };

        PlatformSettings[] detailMap_PlatformSettings =
        {
            new PlatformSettings
            {
                Name = "Standalone",
                Format = TextureImporterFormat.BC7
            },
            new PlatformSettings
            {
                Name = "PS4",
                Format = TextureImporterFormat.BC7
            }
        };

        static void AddPlatformSettings(TextureImporter importer, PlatformSettings[] settings)
        {
            for (var i = 0; i < settings.Length; i++)
                importer.SetPlatformTextureSettings(settings[i].GetPlatformSettings(importer));
        }

        void OnPreprocessTexture()
        {
            var textureImporter = assetImporter as TextureImporter;

            if (!textureImporter)
            {
                return;
            }

            // Game Assets --------------------------------------------------------------------
            if (assetPath.Contains("_Albedo."))
            {
                textureImporter.anisoLevel = 4;
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.sRGBTexture = true;
                textureImporter.streamingMipmaps = true;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;

                AddPlatformSettings(textureImporter, albedoMap_PlatformSettings);
            }
            else if (assetPath.Contains("_MaskMap."))
            {
                textureImporter.anisoLevel = 4;
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.sRGBTexture = false;
                textureImporter.streamingMipmaps = true;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                AddPlatformSettings(textureImporter, maskMap_PlatformSettings);
            }
            else if (assetPath.Contains("_LayerMask."))
            {
                textureImporter.anisoLevel = 4;
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.sRGBTexture = true;
                textureImporter.streamingMipmaps = true;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                AddPlatformSettings(textureImporter, layerMask_PlatformSettings);
            }
            else if (assetPath.Contains("_Normal."))
            {
                textureImporter.anisoLevel = 4;
                textureImporter.textureType = TextureImporterType.NormalMap;
                textureImporter.streamingMipmaps = true;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                AddPlatformSettings(textureImporter, normalMap_PlatformSettings);
            }
            else if (assetPath.Contains("_BNM."))
            {
                textureImporter.anisoLevel = 4;
                textureImporter.textureType = TextureImporterType.NormalMap;
                textureImporter.streamingMipmaps = true;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                AddPlatformSettings(textureImporter, bentnormalMap_PlatformSettings);
            }
            else if (assetPath.Contains("_Height."))
            {
                textureImporter.anisoLevel = 4;
                textureImporter.sRGBTexture = false;
                textureImporter.streamingMipmaps = true;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                AddPlatformSettings(textureImporter, heightMap_PlatformSettings);
            }
            else if (assetPath.Contains("_Thickness."))
            {
                textureImporter.anisoLevel = 4;
                textureImporter.sRGBTexture = false;
                textureImporter.streamingMipmaps = true;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                AddPlatformSettings(textureImporter, thicknessMap_PlatformSettings);
            }
            else if (assetPath.Contains("_Detail."))
            {
                textureImporter.anisoLevel = 4;
                textureImporter.sRGBTexture = false;
                textureImporter.streamingMipmaps = true;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                AddPlatformSettings(textureImporter, detailMap_PlatformSettings);
            }
            
            // Change Android Quality --------------------------------------------------------
            /*
            else if (assetPath.Contains("Assets/Art/Textures"))
            {
                var androidSettings = textureImporter.GetPlatformTextureSettings("Android");
                androidSettings.overridden = true;
                androidSettings.maxTextureSize = 512;
                androidSettings.format = TextureImporterFormat.Automatic;
                androidSettings.compressionQuality = (int)TextureCompressionQuality.Normal;

                textureImporter.SetPlatformTextureSettings(androidSettings);
            }
            */
        }
    }
}
