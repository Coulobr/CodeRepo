// ================================
//  Grubbit Matchmaking Controller (Client)
//  - Creates ticket in casual/ranked queue
//  - Polls TicketStatusResponse until MultiplayAssignment Found
//  - Connects NGO client via UnityTransport to Ip:Port
// ================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Grubbit.Multiplayer
{
    public enum QueueType { Casual, Ranked }

    public class MatchmakingController : MonoBehaviour
    {
        [Header("Queues (Dashboard names)")]
        public string casualQueue = "grubbit-casual";
        public string rankedQueue = "grubbit-ranked";

        [Header("Behavior")]
        public int timeoutSeconds = 60;
        public ushort defaultClientPort = 7777;

        private string _ticketId;
        private CancellationTokenSource _cts;

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public async void StartMatchmaking(QueueType queue, int playerMmr)
        {
            try
            {
	            SceneLoader.Instance.Load(SceneLoader.Scene.MatchmakingScene);

				if (UnityServices.State != ServicesInitializationState.Initialized)
                    await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Steam path is done in bootstrap

                var players = new List<Player>
                {
                    new Player(
                        AuthenticationService.Instance.PlayerId,
                        new Dictionary<string, object>
                        {
                            {"mmr", playerMmr},
                            {"platform", Application.platform.ToString()}
                        }
                    )
                };

                var queueName = queue == QueueType.Casual ? casualQueue : rankedQueue;
                var options = new CreateTicketOptions(queueName: queueName);

                var create = await MatchmakerService.Instance.CreateTicketAsync(players, options);
                _ticketId = create.Id;
                Debug.Log($"[Grubbit] Created ticket: {_ticketId} in '{queueName}'");

                _cts = new CancellationTokenSource();
                StartCoroutine(PollTicketAndConnect(_ticketId, _cts.Token));
            }
            catch (Exception ex)
            {
                Debug.LogError("[Grubbit] StartMatchmaking failed: " + ex.Message);
                ReturnToMenu();
            }
        }

        public async void CancelMatchmaking()
        {
            try
            {
                _cts?.Cancel();
                if (!string.IsNullOrEmpty(_ticketId))
                {
                    await MatchmakerService.Instance.DeleteTicketAsync(_ticketId);
                    _ticketId = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Grubbit] CancelMatchmaking warning: " + ex.Message);
            }
            finally
            {
                ReturnToMenu();
            }
        }

        private IEnumerator PollTicketAndConnect(string ticketId, CancellationToken token)
        {
            float start = Time.realtimeSinceStartup;

            while (!token.IsCancellationRequested && (Time.realtimeSinceStartup - start) < timeoutSeconds)
            {
                var task = MatchmakerService.Instance.GetTicketAsync(ticketId); // Task<TicketStatusResponse>
                while (!task.IsCompleted) yield return null;

                if (task.IsFaulted)
                {
                    Debug.LogError("[Grubbit] GetTicketAsync fault: " + task.Exception);
                    break;
                }

                TicketStatusResponse resp = task.Result;
                if (resp != null && resp.Type == typeof(MultiplayAssignment))
                {
                    var assignment = (MultiplayAssignment)resp.Value;
                    switch (assignment.Status)
                    {
                        case MultiplayAssignment.StatusOptions.Found:
                            string host = string.IsNullOrEmpty(assignment.Ip) ? "127.0.0.1" : assignment.Ip;
                            ushort port = (ushort)(assignment.Port ?? defaultClientPort);
                            Debug.Log($"[Grubbit] Assignment Found: {host}:{port}");
                            yield return ConnectClient(host, port);
                            yield break;

                        case MultiplayAssignment.StatusOptions.Failed:
                        case MultiplayAssignment.StatusOptions.Timeout:
                            Debug.LogWarning($"[Grubbit] Matchmaking ended: {assignment.Status} - {assignment.Message}");
                            ReturnToMenu();
                            yield break;

                        case MultiplayAssignment.StatusOptions.InProgress:
                            // keep polling
                            break;
                    }
                }

                yield return new WaitForSeconds(1f);
            }

            // Timeout or canceled
            ReturnToMenu();
        }

        private IEnumerator ConnectClient(string host, ushort port)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                Debug.LogError("[Grubbit] NetworkManager not found in scene.");
                ReturnToMenu();
                yield break;
            }

            var utp = nm.GetComponent<UnityTransport>();
            if (utp == null)
            {
                Debug.LogError("[Grubbit] UnityTransport missing on NetworkManager.");
                ReturnToMenu();
                yield break;
            }

            utp.SetConnectionData(host, port);

            if (SceneManager.GetActiveScene().name != SceneLoader.Scene.GameScene.ToString())
                SceneLoader.Instance.Load(SceneLoader.Scene.GameScene);
            yield return null;

            nm.StartClient();
        }

        private void ReturnToMenu()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
			SceneLoader.Instance.Load(SceneLoader.Scene.MainMenuScene);
		}
	}
}