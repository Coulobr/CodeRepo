// ================================
//  Grubbit Steam + UGS Bootstrap
//  - Steamworks.NET (optional)
//  - Unity Authentication (Steam sign-in or Anonymous fallback)
// ================================
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Unity.Services.Core;
using Unity.Services.Authentication;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace Grubbit.Multiplayer
{
    public class SteamAndUgsBootstrap : MonoBehaviour
    {
        public static SteamAndUgsBootstrap Instance { get; private set; }

        [Header("Debug")] public bool verbose = true;
        [Tooltip("If Steam isn't available, allow anonymous UGS sign-in for local tests.")]
        public bool allowAnonymousFallback = true;

        #if STEAMWORKS_NET
        private HAuthTicket _steamAuthTicket;
        #if STEAM_WEBAPI_TICKET
        private Callback<GetTicketForWebApiResponse_t> _webApiTicketCb;
        #endif
        #endif

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            try
            {
                #if STEAMWORKS_NET
                if (verbose) Debug.Log("[Grubbit] SteamAPI.Init...");
                if (!Packsize.Test()) Debug.LogError("[Grubbit] Steam Packsize test failed");
                if (!DllCheck.Test()) Debug.LogError("[Grubbit] Steam DllCheck test failed");
                if (!SteamAPI.Init()) throw new Exception("SteamAPI.Init failed. Is Steam running/AppID set?");
                if (verbose) Debug.Log($"[Grubbit] Steam user: {SteamFriends.GetPersonaName()} ({SteamUser.GetSteamID()})");
                #endif

                if (verbose) Debug.Log("[Grubbit] UnityServices.InitializeAsync...");
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    #if STEAMWORKS_NET
                    string ticketHex = await GetSteamSessionTicketHexAsync();
                    if (verbose) Debug.Log($"[Grubbit] Steam ticket hex length: {ticketHex.Length}");
                    await AuthenticationService.Instance.SignInWithSteamAsync(ticketHex, identity: "steam");
                    #else
                    if (!allowAnonymousFallback)
                        throw new Exception("Steam not enabled and anonymous fallback disabled.");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    #endif
                }

                if (verbose) Debug.Log($"[Grubbit] Signed in. PlayerID={AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Grubbit] Bootstrap error: " + ex);
                SceneLoader.Instance.Load(SceneLoader.Scene.MainMenuScene);
            }
        }

        private void OnApplicationQuit()
        {
            #if STEAMWORKS_NET
            if (_steamAuthTicket.m_HAuthTicket != 0)
                SteamUser.CancelAuthTicket(_steamAuthTicket);
            SteamAPI.Shutdown();
            #endif
        }

        #if STEAMWORKS_NET
        private async Task<string> GetSteamSessionTicketHexAsync()
        {
            #if STEAM_WEBAPI_TICKET
            // Newer Steamworks.NET: request WebAPI ticket (bytes arrive via callback)
            var tcs = new TaskCompletionSource<string>();
            _webApiTicketCb = Callback<GetTicketForWebApiResponse_t>.Create(cb =>
            {
                if (cb.m_eResult == EResult.k_EResultOK && cb.m_rgubTicket != null)
                {
                    var sb = new StringBuilder(cb.m_rgubTicket.Length * 2);
                    for (int i = 0; i < cb.m_rgubTicket.Length; i++) sb.AppendFormat("{0:x2}", cb.m_rgubTicket[i]);
                    tcs.TrySetResult(sb.ToString());
                }
                else
                {
                    tcs.TrySetException(new Exception("GetTicketForWebApiResponse failed: " + cb.m_eResult));
                }
            });

            // The identity string should match what your backend expects; "unityauthenticationservice" is fine for UGS.
            _steamAuthTicket = SteamUser.GetAuthTicketForWebApi("unityauthenticationservice");
            return await tcs.Task;
            #else
            // Legacy 3-parameter session ticket (synchronous)
            byte[] buf = new byte[2048];
            uint size;
            _steamAuthTicket = SteamUser.GetAuthSessionTicket(buf, buf.Length, out size);
            Array.Resize(ref buf, (int)size);
            var sb = new StringBuilder(buf.Length * 2);
            for (int i = 0; i < buf.Length; i++) sb.AppendFormat("{0:x2}", buf[i]);
            return sb.ToString();
            #endif
        }
        #endif
    }
}