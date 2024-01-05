using System;
using UnityEditor;

namespace ShaderGlobals
{
    public class ShaderGlobalsDeletionHandler : AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (AssetDatabase.LoadAssetAtPath<ShaderGlobals>(assetPath) is { } globalsToDelete)
            {
                globalsToDelete.ClearValues();
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
