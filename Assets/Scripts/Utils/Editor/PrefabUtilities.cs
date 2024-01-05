using UnityEditor;

namespace Unity.MegacityMetro.EditorTools
{
    public static class PrefabUtilities
    {
        /// <summary>
        /// Helper function to allow reverting prefab changes from selection
        /// </summary>
        [MenuItem("Tools/Prefabs/Revert Selection")]
        static void RevertSelection()
        {
            var gameObjectSelection = Selection.gameObjects;
            Undo.RegisterCompleteObjectUndo(gameObjectSelection, "revert selection");
            foreach (var go in gameObjectSelection)
            {
                if (go != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(go) &&
                    PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                {
                    PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                }
            }
        }

        [MenuItem("Prefabs/Revert Selection (preserve scale)")]
        static void RevertSelectionPreserveScale()
        {
            var gameObjectSelection = Selection.gameObjects;
            Undo.RegisterCompleteObjectUndo(gameObjectSelection, "revert selection");
            foreach (var go in gameObjectSelection)
            {
                if (go != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(go) &&
                    PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                {
                    var t = EditorJsonUtility.ToJson(go.transform);
                    PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                    EditorJsonUtility.FromJsonOverwrite(t, go.transform);
                }
            }
        }
    }
}
