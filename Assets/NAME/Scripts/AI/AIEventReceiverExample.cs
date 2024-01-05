using Unity.NAME.Game;
using UnityEngine;

namespace Unity.NAME.AI
{
    // A simple example for an AI component that receives a specific GameEvent
    // Can be removed

    public class AIEventReceiverExample : MonoBehaviour
    {
        void Awake()
        {
            EventManager.AddListener<ExampleEvent>(OnExampleEvent);
        }

        void OnExampleEvent(ExampleEvent evt)
        {
            Renderer renderer = GetComponent<Renderer>();
            var newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0.2f);
            renderer.sharedMaterial.SetColor("_BaseColor", newColor);
            Debug.Log($"{gameObject.name} received FirstEvent!");
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<ExampleEvent>(OnExampleEvent);
        }
    }
}
