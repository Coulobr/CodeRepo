using System;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Grubbit
{
	[Serializable]
	public class MainMenuUIButtons
	{
		[Header("Config Buttons")]
		[SerializeField] private SmartButton settingsBtn;
		[SerializeField] private SmartButton infoBtn;
		[SerializeField] private SmartButton profileBtn;
		[SerializeField] private SmartButton quitBtn;
		[SerializeField] private SmartButton mailBtn;

		[Header("Frame Buttons")]
		[SerializeField] private SmartButton questsBtn;
		[SerializeField] private SmartButton cosmeticsBtn;
		[SerializeField] private SmartButton playBtn;

		[Header("Other Buttons")]
		[SerializeField] private SmartButton deckBtn;
		[SerializeField] private SmartButton collectionsBtn;
		[SerializeField] private SmartButton geodeBtn;
		[SerializeField] private SmartButton addRubiesBtn;

		[Header("Button Containers")]
		[SerializeField] private RectTransform questsOnHover;
		[SerializeField] private RectTransform questsOnClick;
		[SerializeField] private RectTransform questsDefault;
		[SerializeField] private RectTransform cosmeticsOnHover;
		[SerializeField] private RectTransform cosmeticsOnClick;
		[SerializeField] private RectTransform cosmeticsDefault;
		[SerializeField] private RectTransform playOnHover;
		[SerializeField] private RectTransform playOnClick;
		[SerializeField] private RectTransform playDefault;

		public void Initialize()
		{
			SubscribeToButtonEvents();
			EnableAllButtons();

			// Frame Button Default States
			questsDefault.gameObject.SetActive(true);
			questsOnHover.gameObject.SetActive(false);
			questsOnClick.gameObject.SetActive(false);
			cosmeticsDefault.gameObject.SetActive(true);
			cosmeticsOnHover.gameObject.SetActive(false);
			cosmeticsOnClick.gameObject.SetActive(false);
			playDefault.gameObject.SetActive(true);
			playOnHover.gameObject.SetActive(false);
			playOnClick.gameObject.SetActive(false);
		}

		private void EnableAllButtons()
		{
			settingsBtn.ToggleState(true, false);
			infoBtn.ToggleState(true, false);
			profileBtn.ToggleState(true, false);
			quitBtn.ToggleState(true, false);
			questsBtn.ToggleState(true, false);
			cosmeticsBtn.ToggleState(true, false);
			playBtn.ToggleState(true, false);
			deckBtn.ToggleState(true, false);
			geodeBtn.ToggleState(true, false);
			collectionsBtn.ToggleState(true, false);
			mailBtn.ToggleState(true, false);
			addRubiesBtn.ToggleState(true, false);

			settingsBtn.holdMode = true;
			infoBtn.holdMode = true;
			profileBtn.holdMode = true;
			quitBtn.holdMode = true;
			questsBtn.holdMode = true;
			cosmeticsBtn.holdMode = true;
			playBtn.holdMode = true;
			deckBtn.holdMode = true;
			geodeBtn.holdMode = true;
			collectionsBtn.holdMode = true;
			mailBtn.holdMode = true;
			addRubiesBtn.holdMode = true;
		}

		private void UnsubscribeToButtonEvents()
		{
			// On Click Down
			settingsBtn.onBeginHold.RemoveAllListeners();
			infoBtn.onBeginHold.RemoveAllListeners();
			profileBtn.onBeginHold.RemoveAllListeners();
			quitBtn.onBeginHold.RemoveAllListeners();
			questsBtn.onBeginHold.RemoveAllListeners();
			cosmeticsBtn.onBeginHold.RemoveAllListeners();
			playBtn.onBeginHold.RemoveAllListeners();
			deckBtn.onBeginHold.RemoveAllListeners();
			geodeBtn.onBeginHold.RemoveAllListeners();
			collectionsBtn.onBeginHold.RemoveAllListeners();
			mailBtn.onBeginHold.RemoveAllListeners();
			addRubiesBtn.onBeginHold.RemoveAllListeners();

			// On Click Up
			settingsBtn.onEndHold.RemoveAllListeners();
			infoBtn.onEndHold.RemoveAllListeners();
			profileBtn.onEndHold.RemoveAllListeners();
			quitBtn.onEndHold.RemoveAllListeners();
			questsBtn.onEndHold.RemoveAllListeners();
			cosmeticsBtn.onEndHold.RemoveAllListeners();
			playBtn.onEndHold.RemoveAllListeners();
			deckBtn.onEndHold.RemoveAllListeners();
			geodeBtn.onEndHold.RemoveAllListeners();
			collectionsBtn.onEndHold.RemoveAllListeners();
			mailBtn.onEndHold.RemoveAllListeners();
			addRubiesBtn.onEndHold.RemoveAllListeners();

			// On Hover
			settingsBtn.OnHover_Enter -= OnHoverEnter_SettingsBtn;
			settingsBtn.OnHover_Exit -= OnHoverExit_SettingsBtn;
			infoBtn.OnHover_Enter -= OnHoverEnter_CreditsBtn;
			infoBtn.OnHover_Exit -= OnHoverExit_CreditsBtn;
			profileBtn.OnHover_Enter -= OnHoverEnter_ProfileBtn;
			profileBtn.OnHover_Exit -= OnHoverExit_ProfileBtn;
			quitBtn.OnHover_Enter -= OnHoverEnter_QuitBtn;
			quitBtn.OnHover_Exit -= OnHoverExit_QuitBtn;
			mailBtn.OnHover_Enter -= OnHoverEnter_MailBtn;
			mailBtn.OnHover_Exit -= OnHoverExit_MailBtn;

			addRubiesBtn.OnHover_Enter -= OnHoverEnter_AddRubiesBtn;
			addRubiesBtn.OnHover_Exit -= OnHoverExit_AddRubiesBtn;

			deckBtn.OnHover_Enter -= OnHoverEnter_DecksBtn;
			deckBtn.OnHover_Exit -= OnHoverExit_DecksBtn;

			geodeBtn.OnHover_Enter -= OnHoverEnter_GeodeBtn;
			geodeBtn.OnHover_Exit -= OnHoverExit_GeodeBtn;

			collectionsBtn.OnHover_Enter -= OnHoverEnter_CollectionsBtn;
			collectionsBtn.OnHover_Exit -= OnHoverExit_CollectionsBtn;

			questsBtn.OnHover_Enter -= OnHoverEnter_QuestsBtn;
			questsBtn.OnHover_Exit -= OnHoverExit_QuestsBtn;
			cosmeticsBtn.OnHover_Enter -= OnHoverEnter_CosmeticsBtn;
			cosmeticsBtn.OnHover_Exit -= OnHoverExit_CosmeticsBtn;
			playBtn.OnHover_Enter -= OnHoverEnter_PlayBtn;
			playBtn.OnHover_Exit -= OnHoverExit_PlayBtn;
		}

		private void SubscribeToButtonEvents()
		{
			UnsubscribeToButtonEvents();

			// On Click Down
			settingsBtn.onBeginHold.AddListener(OnPointerDown_SettingsBtn);
			infoBtn.onBeginHold.AddListener(OnPointerDown_CreditsBtn);
			profileBtn.onBeginHold.AddListener(OnPointerDown_ProfileBtn);
			quitBtn.onBeginHold.AddListener(OnPointerDown_QuitBtn);
			questsBtn.onBeginHold.AddListener(OnPointerDown_QuestsBtn);
			cosmeticsBtn.onBeginHold.AddListener(OnPointerDown_CosmeticsBtn);
			playBtn.onBeginHold.AddListener(OnPointerDown_PlayBtn);
			deckBtn.onBeginHold.AddListener(OnPointerDown_DecksBtn);
			geodeBtn.onBeginHold.AddListener(OnPointerDown_GeodeBtn);
			collectionsBtn.onBeginHold.AddListener(OnPointerDown_CollectionsBtn);
			mailBtn.onBeginHold.AddListener(OnPointerDown_MailBtn);
			addRubiesBtn.onBeginHold.AddListener(OnPointerDown_AddRubiesBtn);

			// On Click Up
			settingsBtn.onEndHold.AddListener(OnPointerUp_SettingsBtn);
			infoBtn.onEndHold.AddListener(OnPointerUp_CreditsBtn);
			profileBtn.onEndHold.AddListener(OnPointerUp_ProfileBtn);
			quitBtn.onEndHold.AddListener(OnPointerUp_QuitBtn);
			questsBtn.onEndHold.AddListener(OnPointerUp_QuestsBtn);
			cosmeticsBtn.onEndHold.AddListener(OnPointerUp_CosmeticsBtn);
			playBtn.onEndHold.AddListener(OnPointerUp_PlayBtn);
			deckBtn.onEndHold.AddListener(OnPointerUp_DecksBtn);
			geodeBtn.onEndHold.AddListener(OnPointerUp_GeodeBtn);
			collectionsBtn.onEndHold.AddListener(OnPointerUp_CollectionsBtn);
			mailBtn.onEndHold.AddListener(OnPointerUp_MailBtn);
			addRubiesBtn.onEndHold.AddListener(OnPointerUp_AddRubiesBtn);

			// On Hover
			settingsBtn.OnHover_Enter += OnHoverEnter_SettingsBtn;
			settingsBtn.OnHover_Exit += OnHoverExit_SettingsBtn;
			infoBtn.OnHover_Enter += OnHoverEnter_CreditsBtn;
			infoBtn.OnHover_Exit += OnHoverExit_CreditsBtn;
			profileBtn.OnHover_Enter += OnHoverEnter_ProfileBtn;
			profileBtn.OnHover_Exit += OnHoverExit_ProfileBtn;
			quitBtn.OnHover_Enter += OnHoverEnter_QuitBtn;
			quitBtn.OnHover_Exit += OnHoverExit_QuitBtn;
			mailBtn.OnHover_Enter += OnHoverEnter_MailBtn;
			mailBtn.OnHover_Exit += OnHoverExit_MailBtn;

			addRubiesBtn.OnHover_Enter += OnHoverEnter_AddRubiesBtn;
			addRubiesBtn.OnHover_Exit += OnHoverExit_AddRubiesBtn;

			deckBtn.OnHover_Enter += OnHoverEnter_DecksBtn;
			deckBtn.OnHover_Exit += OnHoverExit_DecksBtn;

			geodeBtn.OnHover_Enter += OnHoverEnter_GeodeBtn;
			geodeBtn.OnHover_Exit += OnHoverExit_GeodeBtn;

			collectionsBtn.OnHover_Enter += OnHoverEnter_CollectionsBtn;
			collectionsBtn.OnHover_Exit += OnHoverExit_CollectionsBtn;

			questsBtn.OnHover_Enter += OnHoverEnter_QuestsBtn;
			questsBtn.OnHover_Exit += OnHoverExit_QuestsBtn;
			cosmeticsBtn.OnHover_Enter += OnHoverEnter_CosmeticsBtn;
			cosmeticsBtn.OnHover_Exit += OnHoverExit_CosmeticsBtn;
			playBtn.OnHover_Enter += OnHoverEnter_PlayBtn;
			playBtn.OnHover_Exit += OnHoverExit_PlayBtn;
		}



		#region Button Events

		private void OnHoverEnter_QuestsBtn(PointerEventData pointerEventData)
		{
			questsDefault.gameObject.SetActive(false);
			questsOnHover.gameObject.SetActive(true);
			questsOnClick.gameObject.SetActive(false);
		}

		private void OnHoverExit_QuestsBtn(PointerEventData pointerEventData)
		{
			questsDefault.gameObject.SetActive(true);
			questsOnHover.gameObject.SetActive(false);
			questsOnClick.gameObject.SetActive(false);
		}

		private void OnHoverEnter_CosmeticsBtn(PointerEventData pointerEventData)
		{
			cosmeticsDefault.gameObject.SetActive(false);
			cosmeticsOnHover.gameObject.SetActive(true);
			cosmeticsOnClick.gameObject.SetActive(false);
		}

		private void OnHoverExit_CosmeticsBtn(PointerEventData pointerEventData)
		{
			cosmeticsDefault.gameObject.SetActive(true);
			cosmeticsOnHover.gameObject.SetActive(false);
			cosmeticsOnClick.gameObject.SetActive(false);
		}

		private void OnHoverEnter_PlayBtn(PointerEventData pointerEventData)
		{
			playDefault.gameObject.SetActive(false);
			playOnHover.gameObject.SetActive(true);
			playOnClick.gameObject.SetActive(false);
		}

		private void OnHoverExit_PlayBtn(PointerEventData pointerEventData)
		{
			playDefault.gameObject.SetActive(true);
			playOnHover.gameObject.SetActive(false);
			playOnClick.gameObject.SetActive(false);
		}

		private void OnHoverEnter_SettingsBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverExit_SettingsBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverEnter_MailBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverExit_MailBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverEnter_CreditsBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverExit_CreditsBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverEnter_ProfileBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverExit_ProfileBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverEnter_QuitBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverExit_QuitBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverEnter_DecksBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverExit_DecksBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverEnter_CollectionsBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverExit_CollectionsBtn(PointerEventData pointerEventData)
		{
		}

		private void OnHoverEnter_GeodeBtn(PointerEventData pointerEventData)
		{
		}
		private void OnHoverExit_GeodeBtn(PointerEventData pointerEventData)
		{
		}
		private void OnHoverEnter_AddRubiesBtn(PointerEventData pointerEventData)
		{
		}
		private void OnHoverExit_AddRubiesBtn(PointerEventData pointerEventData)
		{
		}

		private void OnPointerDown_PlayBtn()
		{
			playDefault.gameObject.SetActive(false);
			playOnHover.gameObject.SetActive(false);
			playOnClick.gameObject.SetActive(true);
		}

		private void OnPointerDown_CosmeticsBtn()
		{
			cosmeticsDefault.gameObject.SetActive(false);
			cosmeticsOnHover.gameObject.SetActive(false);
			cosmeticsOnClick.gameObject.SetActive(true);
		}

		private void OnPointerDown_QuestsBtn()
		{
			questsDefault.gameObject.SetActive(false);
			questsOnHover.gameObject.SetActive(false);
			questsOnClick.gameObject.SetActive(true);
		}

		private void OnPointerDown_QuitBtn()
		{
		}
		private void OnPointerDown_ProfileBtn()
		{
		}
		private void OnPointerDown_CreditsBtn()
		{
		}
		private void OnPointerDown_SettingsBtn()
		{
		}
		private void OnPointerDown_DecksBtn()
		{
		}
		private void OnPointerDown_CollectionsBtn()
		{
		}

		private void OnPointerDown_GeodeBtn()
		{
			MainMenuUI.Instance.openGeodeDropdownPanel?.Invoke();
		}

		private void OnPointerDown_AddRubiesBtn()
		{
		}
		private void OnPointerDown_MailBtn()
		{
		}

		private void OnPointerUp_PlayBtn()
		{
			if (!playBtn.hovering)
			{
				return;
			}

			playDefault.gameObject.SetActive(false);
			playOnHover.gameObject.SetActive(true);
			playOnClick.gameObject.SetActive(false);
			MainMenuUI.Instance.openSidePanel?.Invoke(GrubbitEnums.MainMenuSidePanelState.Play);
		}

		private void OnPointerUp_CosmeticsBtn()
		{
			if (!cosmeticsBtn.hovering)
			{
				return;
			}

			cosmeticsDefault.gameObject.SetActive(false);
			cosmeticsOnHover.gameObject.SetActive(true);
			cosmeticsOnClick.gameObject.SetActive(false);
			MainMenuUI.Instance.openSidePanel?.Invoke(GrubbitEnums.MainMenuSidePanelState.Cosmetics);
		}

		private void OnPointerUp_QuestsBtn()
		{
			if (!questsBtn.hovering)
			{
				return;
			}

			questsDefault.gameObject.SetActive(false);
			questsOnHover.gameObject.SetActive(true);
			questsOnClick.gameObject.SetActive(false);
			MainMenuUI.Instance.openSidePanel?.Invoke(GrubbitEnums.MainMenuSidePanelState.Quests);
		}

		private void OnPointerUp_QuitBtn()
		{
			if (!quitBtn.hovering)
			{
				return;
			}

			MainMenuUI.Instance.PromptApplicationQuit();
		}

		private void OnPointerUp_ProfileBtn()
		{
			if (!profileBtn.hovering)
			{
				return;
			}
		}

		private void OnPointerUp_CreditsBtn()
		{
			if (!infoBtn.hovering)
			{
				return;
			}
		}

		private void OnPointerUp_SettingsBtn()
		{
			if (!settingsBtn.hovering)
			{
				return;
			}
		}
		private void OnPointerUp_DecksBtn()
		{
			if (!deckBtn.hovering)
			{
				return;
			}
		}
		private void OnPointerUp_CollectionsBtn()
		{
			if (!collectionsBtn.hovering)
			{
				return;
			}
		}

		private void OnPointerUp_GeodeBtn()
		{
			if (!geodeBtn.hovering)
			{
				return;
			}
		}

		private void OnPointerUp_AddRubiesBtn()
		{
			if (!addRubiesBtn.hovering)
			{
				return;
			}
		}

		private void OnPointerUp_MailBtn()
		{
			if (!mailBtn.hovering)
			{
				return;
			}
		}
		#endregion
	}
}


