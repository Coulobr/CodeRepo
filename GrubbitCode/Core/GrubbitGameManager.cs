using System;
using System.Threading.Tasks;
using TMPro;
using DG.Tweening;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Multiplayer;
using UnityEngine.AddressableAssets;
using UnityEngine.Analytics;
using UnityEngine.Jobs;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Grubbit
{
	public class GrubbitGameManager : GrubbitSingleton<GrubbitGameManager>
	{
		public bool isDemo = true;
		public SessionNetworkEvents SessionNetworkEvents { get; private set; }
		public NetworkManager unityNetworkManager;
		public SessionsManager sessionsManager;
		public SteamManager steamManager;
		public SceneLoader sceneLoader;

		protected override void Awake()
		{
			base.Awake();
			GrubbitGlobals.LoadGlobals();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		private async void Start()
		{
			await InstantiateAsync(sessionsManager);
			//await InstantiateAsync(steamManager);
			await InstantiateAsync(unityNetworkManager);
			await InstantiateAsync(sceneLoader);

			SessionNetworkEvents = GetComponent<SessionNetworkEvents>();
			SceneLoader.Instance.Load(SceneLoader.Scene.MainMenuScene, true, LoadMainMenu);
		}

		private void LoadMainMenu()
		{
			SessionsManager.Instance.LoadSession<MainMenuSession>(false);
		}
	}
}