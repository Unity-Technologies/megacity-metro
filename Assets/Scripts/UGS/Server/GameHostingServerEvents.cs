using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Samples.GameServerHosting
{
    public static class GameHostingServerEvents
    {
        public static Action<string> UserJoinedServer;
        public static Action<string> UserLeftServer;
        public static Action<string> MapChanged;
        public static Action<string> ModeChanged;
    }
}
