using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Samples
{
    /// <summary>
    /// The Samples Plugins implementation of the Unity Authentication Service
    /// Using this ensures we don't log in multiple times when combining samples.
    /// </summary>
    public class PlayerAuthentication
    {
        [field: SerializeField] public PlayerProfile LocalPlayer { get; private set; }
        public static bool IsSignedIn => UnityServices.State != ServicesInitializationState.Uninitialized && AuthenticationService.Instance.IsSignedIn;
        public static string PlayerId => IsSignedIn ? AuthenticationService.Instance.PlayerId : "";

        public async Task Init(string profileName = null)
        {
            if (profileName != null)
            {
                // ProfileNames can't contain non-alphanumeric characters
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                profileName = rgx.Replace(profileName, "");
                var profileHash = GenerateRandomUniqueHash(profileName);
                var authProfile = new InitializationOptions().SetProfile(profileHash);
                // If you are using multiple unity services, make sure to initialize it only once before using your services.
                await UnityServices.InitializeAsync(authProfile);
            }
            else {
                
                await UnityServices.InitializeAsync();
            }

            Debug.Log($"[SERVICES] Initializing Unity Services {UnityServices.Instance.State}");
        }

        public async Task SignIn(string profileName = null)
        {
            if (IsSignedIn)
                return;
            await Init(profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var playerName = profileName ?? "default";
            var playerID = AuthenticationService.Instance.PlayerId;

            LocalPlayer = new PlayerProfile(playerName, playerID);
        }

        public static string GenerateRandomUniqueHash(string profileName)
        {
            // Add more randomness using Guid and RNGCryptoServiceProvider
            Guid guid = Guid.NewGuid();
            byte[] randomBytes = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            // Incorporate profile name, time, and randomness
            profileName += DateTime.Now.Ticks + BitConverter.ToString(randomBytes) + guid.ToString();

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(profileName);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Use Base64 for encoding but ensure to keep it URL-safe
                string base64Hash = Convert.ToBase64String(hashBytes, 0, 24);
                return base64Hash.Replace("/", "").Replace("+", "").Substring(0, 16);
            }
        }
    }
}
