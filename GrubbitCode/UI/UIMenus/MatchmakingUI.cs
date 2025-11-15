using UnityEngine;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using DG.Tweening;
using Grubbit;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor.Rendering.LookDev;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Grubbit
{
	public class MatchmakingUI : Menu<MatchmakingUI>
	{
		public Button cancelSearchButton;
		public Button acceptButton;
		public Button declineButton;
		public TextMeshProUGUI timerText;
		public TextMeshProUGUI searchingText;
		public TextMeshProUGUI tipText;
		public GameObject frogLeapAnimation;

		private int timerSeconds;
		private int timerMinutes;

		private IEnumerator timerCoroutine;

		public override void InternalOpen()
		{
			base.InternalOpen();

			searchingText.text = "Searching";
			frogLeapAnimation.SetActive(true);
			cancelSearchButton.gameObject.SetActive(true);
			cancelSearchButton.onClick.AddListener(OnCancelSearch);
			acceptButton.onClick.AddListener(OnAcceptMatch);
			declineButton.onClick.AddListener(OnDeclineMatch);

			timerCoroutine = null;
			timerCoroutine = Co_StartTimer();
			StartCoroutine(timerCoroutine);
		}

		private void OnDeclineMatch()
		{
			Close();
		}

		private void OnAcceptMatch()
		{
			SceneLoader.Instance.Load(SceneLoader.Scene.GameScene, true);
		}

		public override void InternalClose()
		{
			base.InternalClose();
			cancelSearchButton.onClick.RemoveAllListeners();
			acceptButton.onClick.RemoveAllListeners();
			declineButton.onClick.RemoveAllListeners();

			if (timerCoroutine != null)
			{
				StopCoroutine(timerCoroutine);
				timerCoroutine = null;
			}

			SceneLoader.Instance.Load(SceneLoader.Scene.MainMenuScene);
		}

		private IEnumerator Co_StartTimer()
		{
			var secondWait = new WaitForSecondsRealtime(1);
			timerText.text = "00:00";
			while (timerMinutes < 100)
			{
				yield return secondWait;
				timerSeconds++;
				if (timerSeconds == 60)
				{
					timerSeconds = 0;
					timerMinutes++;
				}

				UpdateTimerText();
			}
			yield return null;
		}

		private void OnMatchFound()
		{
			searchingText.text = "Match Found!";
			frogLeapAnimation.SetActive(false);

			if (timerCoroutine != null)
			{
				StopCoroutine(timerCoroutine);
				timerCoroutine = null;
			}

			//cancelSearchButton.gameObject.SetActive(false);
			//acceptButton.gameObject.SetActive(true);
			//declineButton.gameObject.SetActive(true);
		}

		private void OnCancelSearch()
		{
			Close();
		}

		private void UpdateTimerText()
		{
			timerText.text = $"{timerMinutes:00}:{timerSeconds:00}";
		}
	}
}



