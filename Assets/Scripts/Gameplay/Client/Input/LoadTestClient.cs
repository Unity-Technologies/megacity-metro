using System;
using System.Threading;
using Unity.Mathematics;
using Unity.MegacityMetro.Gameplay;
using Unity.MegacityMetro.UGS;
using Unity.MegacityMetro.UI;
using Unity.NetCode.Extensions;
using UnityEngine;

namespace CipSoft.LoadTest
{
    public class LoadTestClient : MonoBehaviour
    {
        public MatchMakingConnector MatchMakingConnector;
        public MainMenu MainMenu;
        public static bool debug = false;
        public static bool IsLoadTest = false;
        private ushort port = 5010;
        private string ip = "192.168.200.229";
        private float millisecondsToWait = 0; 

        void Awake()
        {
            MainMenu.OnGameModeSelected += (GameMode gameMode) =>
            {
                if (gameMode == GameMode.Multiplayer)
                {
                    ChangeIPAndPort();
                }
            };
            string[] args = Environment.GetCommandLineArgs();

            Debug.Log("Command line arguments: " + string.Join(" ", args));

            //get port and ip from command line
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-port")
                {
                    port = ushort.Parse(args[i + 1]);
                    Debug.Log("Set port to " + port);
                }
                if (args[i] == "-ip")
                {
                    ip = args[i + 1];
                    //networkAddress = ip;
                    Debug.Log("Set ip to " + ip);
                }
    #if !UNITY_SERVER
                if (args[i] == "-loadtest" || debug)
                {
                    IsLoadTest = true;
                    Debug.LogWarning("Load test mode enabled");
                }
    #endif
            }

            ChangeIPAndPort();
        }

        void Start()
        {
            if (IsLoadTest)
            {
                try
                {
                    MatchMakingConnector.Instance.SetProfileServiceName(BotNameGenerator.GetRandomName());
                }
                catch (Exception e)
                {
                    Debug.LogError("Error setting profile service name: " + e.Message);
                }
                MatchMakingConnector.Instance.ConnectToServer();
                return;
            }
        }

        void Update()
        {
            if (IsLoadTest)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 8;

                var overflow = 1.0/Application.targetFrameRate - Time.deltaTime;

                if (overflow > 0.0)
                    millisecondsToWait++;
                else
                    millisecondsToWait--;
                
                Thread.Sleep( (int)Mathf.Clamp(millisecondsToWait, 0.0f, 1.0f/Application.targetFrameRate * 1000.0f) ); // clamp for sanity
            }
        }

        void ChangeIPAndPort()
        {
            MatchMakingConnector.SetIPAndPort(ip, port);
            Debug.Log("Set IP and port to " + ip + ":" + port);
        }
    }
}