using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.MegacityMetro.UI  
{
    public class AssignCameraInput : MonoBehaviour
    {
        private PlayerInput m_PlayerInput;
        
        private void Awake()
        {
            m_PlayerInput = GetComponent<PlayerInput>();
        }
    
        private void Start()
        {
            m_PlayerInput.camera = Camera.main;
        }
    }
}
