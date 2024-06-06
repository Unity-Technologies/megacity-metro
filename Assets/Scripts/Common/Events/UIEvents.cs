using System;
using UnityEngine;

namespace Unity.MegacityMetro.UI
{
    public static class UIEvents
    {
        // UI Input Events
        public static Action<Vector2> OnNavigate;
        public static Action OnSubmit;
        public static Action OnCancel;
        
        // UI Game Events
        public static Action<bool> OnPauseOptionsShown;
    }
}