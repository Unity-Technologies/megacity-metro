using Unity.NAME.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.NAME.UI
{
    // A script to trigger a menu event
    public class UserMenuActionButton : MonoBehaviour
    {
        public UserMenuAction UserAction;

        public void TriggerMenuEvent()
        {
            UserMenuActionEvent evt = Events.UserMenuActionEvent;
            evt.UserMenuAction = UserAction;
            EventManager.Broadcast(evt);
        }
    }
}
