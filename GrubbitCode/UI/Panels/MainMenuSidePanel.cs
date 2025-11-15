using System;
using UnityEngine;
using UnityEngine.UI;

namespace Grubbit
{
	public class MainMenuSidePanel : Panel<MainMenuSidePanel>
	{
		public bool IsExpanded => GetComponent<RectTransform>().anchoredPosition.x == 0;

		[Header("States")]
		public GameObject playState;
		public GameObject questsState;
		public GameObject cosmeticsState;

		[Header("Play State Buttons")] 
		public SmartButton casualButton;
		public SmartButton rankedButton;
		public SmartButton deckSelectButton;
		public SmartButton playButton;
		public Toggle formatToggle;

		private GrubbitEnums.MatchmakingMode currentMatchmakingMode;
		private GrubbitEnums.GrubbitGameFormat currentGameFormat;


		protected override void OnOpened()
		{
			base.OnOpened();
			SetDefaultState();
			SetDefaultPosition();
		}

		private void SetDefaultState()
		{
			currentGameFormat = GrubbitEnums.GrubbitGameFormat.Default;
			currentMatchmakingMode = GrubbitEnums.MatchmakingMode.Casual;
			UpdatePanel(GrubbitEnums.MainMenuSidePanelState.Play);
		}

		public void UpdatePanel(GrubbitEnums.MainMenuSidePanelState desiredState)
		{
			base.UpdatePanel();
			ChangeState(desiredState);
		}

		private void ChangeState(GrubbitEnums.MainMenuSidePanelState desiredState)
		{
			playState.gameObject.SetActive(desiredState == GrubbitEnums.MainMenuSidePanelState.Play);
			questsState.gameObject.SetActive(desiredState == GrubbitEnums.MainMenuSidePanelState.Quests);
			cosmeticsState.gameObject.SetActive(desiredState == GrubbitEnums.MainMenuSidePanelState.Cosmetics);
			UnsubscribeToButtonEvents();

			switch (desiredState)
			{
				case GrubbitEnums.MainMenuSidePanelState.Play:
					casualButton.OnToggled += Casual_OnToggled;
					rankedButton.OnToggled += Ranked_OnToggled;
					playButton.onClick.AddListener(Play_OnClick);
					deckSelectButton.onClick.AddListener(Deck_OnClick);
					formatToggle.onValueChanged.AddListener(Toggle_OnValueChanged);
					break;
				case GrubbitEnums.MainMenuSidePanelState.Quests:
					break;
				case GrubbitEnums.MainMenuSidePanelState.Cosmetics:
					break;
			}
		}

		private void SetDefaultPosition()
		{
			var rect = GetComponent<RectTransform>();
			rect.anchoredPosition = new Vector2(600f, rect.anchoredPosition.y);
		}

		private void UnsubscribeToButtonEvents()
		{
			playButton.onClick.RemoveAllListeners();
			deckSelectButton.onClick.RemoveAllListeners();
			formatToggle.onValueChanged.RemoveAllListeners();
		}

		private void Casual_OnToggled(bool desiredState)
		{
			currentMatchmakingMode = GrubbitEnums.MatchmakingMode.Casual;
		}

		private void Ranked_OnToggled(bool desiredState)
		{
			currentMatchmakingMode = GrubbitEnums.MatchmakingMode.Ranked;
		}

		private void Play_OnClick()
		{
			MainMenuUI.Instance.OnPlayMatchPressed(new Tuple<GrubbitEnums.MatchmakingMode, GrubbitEnums.GrubbitGameFormat>(currentMatchmakingMode, currentGameFormat));
		}

		private void Deck_OnClick()
		{
			if (!deckSelectButton.hovering)
			{
				return;
			}

			MainMenuUI.Instance.changeDeckPressed?.Invoke();
		}

		private void Toggle_OnValueChanged(bool state)
		{
			currentGameFormat = state ? GrubbitEnums.GrubbitGameFormat.Leap : GrubbitEnums.GrubbitGameFormat.Default;
		}
	}
}