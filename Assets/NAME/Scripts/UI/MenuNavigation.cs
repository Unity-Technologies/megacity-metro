using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.NAME.UI
{
    // A script that allows to target a specific Selectable
    // It is used in various menus to already have to main button selected.
    // This allows the UI navigation with WASD and the arrow keys

    public class MenuNavigation : MonoBehaviour
    {
        public Selectable DefaultSelection;
        public bool ForceSelection = false;

        void Start()
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null || ForceSelection)
            {
                EventSystem.current.SetSelectedGameObject(DefaultSelection.gameObject);
            }
        }

        void OnDisable()
        {
            if (ForceSelection && EventSystem.current.currentSelectedGameObject == DefaultSelection.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}
