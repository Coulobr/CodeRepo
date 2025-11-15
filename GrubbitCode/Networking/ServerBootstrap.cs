// ================================
//  Grubbit Dedicated Server Bootstrap (Multiplay)
// ================================
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Grubbit.Multiplayer
{
    public class ServerBootstrap : MonoBehaviour
    {
        [Header("Server")]
        public ushort listenPort = 7777;
        public string buildVersion = "1.0.0";
        public string serverName = "Grubbit-Server";
        public ushort maxPlayers = 2;

        private IServerQueryHandler _sqp;

        private async void Start()
        {
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);

            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Grubbit] Server UGS init warning: " + e.Message);
            }

            var nm = NetworkManager.Singleton;
            var utp = nm.GetComponent<UnityTransport>();

            try
            {
                var cfg = MultiplayService.Instance.ServerConfig;
                if (cfg != null && cfg.Port > 0)
                {
                    listenPort = (ushort)cfg.Port;
                }
            }
            catch { /* local */ }

            utp.SetConnectionData("0.0.0.0", listenPort, "0.0.0.0");

            try
            {
                _sqp = await MultiplayService.Instance.StartServerQueryHandlerAsync(
                    maxPlayers,
                    serverName,
                    buildVersion,
                    "Default",
                    "Arena01");
                UpdateSqp(0);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Grubbit] SQP start failed: " + e.Message);
            }

            SceneLoader.Instance.Load(SceneLoader.Scene.MainMenuScene);
            nm.StartServer();
        }

        public void UpdateSqp(ushort currentPlayers)
        {
            if (_sqp == null) return;
            _sqp.CurrentPlayers = currentPlayers;
            _sqp.MaxPlayers = maxPlayers;
            _sqp.UpdateServerCheck();
        }

        private void OnDestroy()
        {
            _sqp?.Dispose();
        }
    }
}