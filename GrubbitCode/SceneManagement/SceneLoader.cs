using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grubbit;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkSceneManager;

public class SceneLoader : GrubbitSingleton<SceneLoader>
{
	public Action onSceneLoaded = null;
	private AsyncOperation loadingAsyncOperation;

	public enum Scene
	{
		LoadingScene,
		Splash,
		MainMenuScene,
		GameScene,
		MatchmakingScene
	}

	protected override void Awake()
	{	
		base.Awake();
	}

	public void Load(Scene scene, bool showLoadingScreen = true, Action onCompleteCallback = null)
	{
		if (!gameObject.activeSelf)
		{
			gameObject.SetActive(true);
		}

		StartCoroutine(Co_LoadScene(scene, showLoadingScreen, onCompleteCallback));
	}

	private IEnumerator Co_LoadScene(Scene scene, bool showLoadingScreen = true, Action onCompleteCallback = null)
	{
		if (onSceneLoaded == null)
		{
			if (onCompleteCallback != null)
			{
				onSceneLoaded = onCompleteCallback;
			}

			if (showLoadingScreen)
			{
				SceneManager.LoadScene((int)Scene.LoadingScene);
				yield return new WaitForSeconds(3);
			}

			SceneManager.LoadScene((int)scene);

			if (onCompleteCallback != null)
			{
				onSceneLoaded?.Invoke();
			}

			onSceneLoaded = null;
		}
		yield return null;
	}

	public async void NetworkLoad(Scene scene)
	{
		if (NetworkManager.Singleton.IsClient)
		{
			if (NetworkManager.Singleton != null)
			{
				NetworkManager.Singleton.SceneManager.OnLoad += OnSceneLoadStarted;
				NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadCompleted;
				NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnServerAllClientsLoaded;
			}

			await SceneManager.LoadSceneAsync((int)Scene.LoadingScene);
			await Task.Delay(50);
		}

		NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Additive);
	}

	public void Server_LoadGameScene()
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}

		NetworkManager.Singleton.SceneManager.LoadScene(Scene.GameScene.ToString(), LoadSceneMode.Additive);
	}

	public float GetLoadingProgress()
	{
		if (loadingAsyncOperation != null)
		{
			return loadingAsyncOperation.progress;
		}
		return 0;
	}

	private void OnSceneLoadStarted(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOp)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Debug.Log($"SERVER: Loading Scene({sceneName})");
		}

		if (NetworkManager.Singleton.IsConnectedClient)
		{
			Debug.Log($"CLIENT: Loading Scene({sceneName})");
			loadingAsyncOperation = asyncOp;
		}
	}

	private void OnServerAllClientsLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Debug.Log($"SERVER: All Clients Finished Loaded Scene({sceneName})");
			Debug.Log($"SERVER: # Clients Loaded({clientsCompleted?.Count})");
			Debug.Log($"SERVER: # Clients Timed Out({clientsTimedOut?.Count})");
		}

		if (NetworkManager.Singleton.IsClient)
		{
			Debug.Log($"CLIENT: Finished Loading Scene:{sceneName}");
			Debug.Log($"CLIENT: # Clients Loaded({clientsCompleted?.Count})");
			Debug.Log($"CLIENT: # Clients Timed Out({clientsTimedOut?.Count})");
			SceneManager.UnloadSceneAsync((int)Scene.LoadingScene);
		}

		if (clientsTimedOut?.Count > 0)
		{
			Debug.LogError("One or more players didn't connect");
		}
	}

	private void OnSceneLoadCompleted(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Debug.Log($"SERVER: Client({clientId}) Finished Loading Scene({sceneName})");
		}

		if (NetworkManager.Singleton.IsConnectedClient)
		{
			Debug.Log($"CLIENT: Finished Loading Scene({sceneName})");
		}
	}

}