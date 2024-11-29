using System;
using System.Linq;
using JetBrains.Annotations;
using Unity.Networking.Transport;
using UnityEngine;

namespace Unity.MegacityMetro
{
    /// <summary>
    /// Parses and exposes commandline options as properties.
    /// </summary>
    public static class CommandLineConfig
    {
        /// <summary><para>Expects the following format: '--enable-thin-clients [port]`:</para>
        /// <para>- Running a client with this arg - while in headless mode - will trigger automatic matchmaking flows.</para>
        /// <para>- Otherwise, on a rendering client, ThinClients will follow the user's UI choice.</para>
        /// </summary>
        internal const string ThinClientArg = "--enable-thin-clients";

        /// <summary>Expects the following format: '--max-cars [count]`.</summary>
        private const string MaxCarsArg = "--max-cars";

        /// <summary>Expects the following format: '-port [ushort_port]`.</summary>
        private const string MultiplayPortArg = "-port";

        private const string HardcodedMultiplayServerIp = "0.0.0.0";
        private const string MultiplayDeveloperApiKeyIdentifierArg = "--api-key-id";
        private const string MultiplayDeveloperApiKeySecretArg = "--api-key-secret";

        public static bool AutomaticallyAddThinClients => TargetThinClientWorldCount > 0;
        public static bool AutomaticallyMatchmake => Application.isBatchMode;
        public static NetworkEndpoint MultiplayEndpoint { get; private set; }
        public static string MultiplayDeveloperApiKeyIdentifier { get; private set; }
        public static string MultiplayDeveloperApiKeySecret { get; private set; }
        public static int TargetThinClientWorldCount { get; private set; }
        public static int MaxCarCount { get; private set; }

        static CommandLineConfig()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
#if UNITY_SERVER
            //If we are running Multiplay UGS, it needs to set to endpoint specific to Multiplay
            ParseMultiplayEndpoint(commandLineArgs);
#else
            ParseThinClientArgs(commandLineArgs);
            ParseMaxCarCountArgs(commandLineArgs);
            
            
            // Warn about incorrect use of -batchmode.
            if (!Application.isBatchMode)
            {
                if (AutomaticallyMatchmake)
                    Debug.LogWarning($"[CLI] Warning: Detected {ThinClientArg} so automatically matchmaking, but not running headless!");
            }
#endif
        }

        [UsedImplicitly]
        private static void ParseThinClientArgs(string[] commandLineArgs)
        {
            // Uncomment to test CLI thin client args.
            //commandLineArgs = commandLineArgs.Concat(new [] { ThinClientArg, "2" }).ToArray();

            int userSpecifiedNumThinClients = 0;

            var indexOfThinClientCount = Array.IndexOf(commandLineArgs, ThinClientArg);
            var hasThinClientSpecified = indexOfThinClientCount != -1;

            // No ThinClientsEnabled arg specified. ThinClients are disabled.
            if (!hasThinClientSpecified)
                return;

            var indexOfCountString = indexOfThinClientCount + 1;
            if (indexOfCountString < commandLineArgs.Length)
            {
                var countString = commandLineArgs[indexOfCountString];
                if (!int.TryParse(countString, out userSpecifiedNumThinClients))
                    Debug.LogError(
                        $"[CLI] Cannot parse commandline arg {indexOfCountString} '{countString}' as int count (for arg '{ThinClientArg}')!");
            }
            else
                Debug.LogError(
                    $"[CLI] Expecting an int count arg after commandline arg '{ThinClientArg}' but there was none!");

            TargetThinClientWorldCount = userSpecifiedNumThinClients;
            Debug.Log(
                $"[CLI] ThinClients enabled via commandline arg '{ThinClientArg} '{userSpecifiedNumThinClients}'. Behaviour: {(AutomaticallyMatchmake ? "AutoMatchmake" : "Follow Main Client")}.");
        }

        [UsedImplicitly]
        private static void ParseMaxCarCountArgs(string[] commandLineArgs)
        {
            var indexOfMaxCarCount = Array.IndexOf(commandLineArgs, MaxCarsArg);

            // No MaxCars arg specified. Leaving default.
            if (indexOfMaxCarCount == -1)
                return;

            var indexOfMaxCarInt = indexOfMaxCarCount + 1;
            if (indexOfMaxCarInt >= commandLineArgs.Length)
            {
                Debug.LogError($"[CLI] Expecting an int after commandline arg '{MaxCarsArg}' but there was none!");
                return;
            }

            var maxCarString = commandLineArgs[indexOfMaxCarInt];
            if (!int.TryParse(maxCarString, out var maxCarCount))
            {
                Debug.LogError(
                    $"[CLI] Cannot parse commandline arg {indexOfMaxCarInt} '{maxCarString}' as int (for arg '{MaxCarsArg}')! Leaving existing value.");
                return;
            }

            MaxCarCount = maxCarCount;
            Debug.Log(
                $"[CLI] Successfully parsed commandline arg {indexOfMaxCarCount} '{maxCarString}' as int '{maxCarCount}' (for arg '{MaxCarsArg}')!");
        }

        [UsedImplicitly]
        private static void ParseMultiplayEndpoint(string[] commandlineArgs)
        {
            var hardcodedFallbackAddress = NetCodeBootstrap.MegacityMetroServerIp;
            MultiplayEndpoint = hardcodedFallbackAddress;

            var indexOfPort = Array.IndexOf(commandlineArgs, MultiplayPortArg);
            if (indexOfPort == -1 || indexOfPort >= commandlineArgs.Length)
            {
                Debug.LogWarning(
                    $"[CLI] Expecting commandline arg '{MultiplayPortArg}' followed by a port, but not set! Using hardcodedFallbackAddress '{hardcodedFallbackAddress}'!");
                return;
            }

            var portString = commandlineArgs[indexOfPort + 1];
            if (!ushort.TryParse(portString, out var portShort))
            {
                Debug.LogError($"[CLI] Cannot parse '{MultiplayPortArg}' arg: '{portString}' not a ushort!");
                return;
            }

            if (!NetworkEndpoint.TryParse(HardcodedMultiplayServerIp, portShort, out var parsedEndpoint,
                    NetworkFamily.Ipv4))
            {
                if (!NetworkEndpoint.TryParse(HardcodedMultiplayServerIp, portShort, out parsedEndpoint,
                        NetworkFamily.Ipv6))
                {
                    Debug.LogError(
                        $"[CLI] Cannot parse code constant {nameof(HardcodedMultiplayServerIp)} (value: '{HardcodedMultiplayServerIp}:{portShort}') as IPv4/6 NetworkEndpoint, for {nameof(MultiplayEndpoint)} config! Using hardcoded address '{hardcodedFallbackAddress}' instead!");
                    return;
                }
            }

            Debug.Log($"[CLI] Found and correctly parsed '{MultiplayPortArg}' arg as '{parsedEndpoint.Address}'!");
            MultiplayEndpoint = parsedEndpoint;

            // Used for testing DGS builds locally
            var indexOfApiIdentifier = Array.IndexOf(commandlineArgs, MultiplayDeveloperApiKeyIdentifierArg);
            if (indexOfApiIdentifier > -1)
            {
                MultiplayDeveloperApiKeyIdentifier = commandlineArgs[indexOfApiIdentifier + 1];
            }

            var indexOfApiSecret = Array.IndexOf(commandlineArgs, MultiplayDeveloperApiKeySecretArg);
            if (indexOfApiSecret > -1)
            {
                MultiplayDeveloperApiKeySecret = commandlineArgs[indexOfApiSecret + 1];
            }
        }
    }
}