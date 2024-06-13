using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    public class GameVersion : MonoBehaviour
    {
        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var label = root.Q<Label>("version");
            label.text = $"v.{Application.version}";
        }
    }
}