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
using Grubbit.Multiplayer;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Grubbit
{
	public class MainMenuUI : Menu<MainMenuUI>
	{
		public RectTransform sidePanelContainer;
		public RectTransform headerContainer;
		public MainMenuUIButtons uiButtons;

		// Events
		public Action<GrubbitEnums.MainMenuSidePanelState> openSidePanel;
		public Action<Tuple<GrubbitEnums.MatchmakingMode, GrubbitEnums.GrubbitGameFormat>> playPressed;
		public Action changeDeckPressed;
		public Action openGeodeDropdownPanel;


		// Vars
		private GrubbitEnums.MainMenuSidePanelState currentSidePanelState;
		private MainMenuSidePanel sidePanel;
		private GeodeDropdownPanel geodeDropdownPanel;
		private bool panelsGenerated = false;

		public MatchmakingController matchmakingController;

		private int GetPlayerMmr(QueueType t) => 1500; // TODO: load from Cloud Save or backend
		public void OnPlayCasual() => matchmakingController.StartMatchmaking(QueueType.Casual, GetPlayerMmr(QueueType.Casual));
		public void OnPlayRanked() => matchmakingController.StartMatchmaking(QueueType.Ranked, GetPlayerMmr(QueueType.Ranked));
		public void OnCancel() => matchmakingController.CancelMatchmaking();

		[ContextMenu("Generate Panels")]
		public void Debug_Create()
		{
			if (!panelsGenerated)
			{
				GeneratePanels();
				uiButtons.Initialize();

				openSidePanel = null;
				playPressed = null;
				changeDeckPressed = null;

				openSidePanel += OnOpenSidePanel;
				playPressed += OnPlayMatchPressed;
				changeDeckPressed += OnChangeDeckBtnPressed;
			}

			GrubbitCardUtility.LoadAllCards();
		}

		public override void InternalOpen()
		{
			base.InternalOpen();
			uiButtons.Initialize();

			openSidePanel = null;
			playPressed = null;
			changeDeckPressed = null;

			openSidePanel += OnOpenSidePanel;
			playPressed += OnPlayMatchPressed;
			changeDeckPressed += OnChangeDeckBtnPressed;
			openGeodeDropdownPanel += OnOpenGeodeDropdown;
		}

		public override void InternalClose()
		{
			base.InternalClose();
		}

		public override void GeneratePanels()
		{
			base.GeneratePanels();
			sidePanel = MainMenuSidePanel.Create(sidePanelContainer);
			sidePanel.Open(true);
			geodeDropdownPanel = GeodeDropdownPanel.Create(headerContainer);
			sidePanel.Open(true);
			panelsGenerated = true;
		}

		private void OnOpenSidePanel(GrubbitEnums.MainMenuSidePanelState desiredState)
		{
			if (sidePanel.IsExpanded)
			{
				sidePanel.GetComponent<RectTransform>().DOAnchorPosX(600, .33f).SetEase(Ease.OutQuad)
					.OnComplete(() =>
					{
						sidePanel.UpdatePanel(desiredState);
						sidePanel.GetComponent<RectTransform>().DOAnchorPosX(0, .33f).SetEase(Ease.InQuad);
					});
			}
			else
			{
				sidePanel.UpdatePanel(desiredState);
				sidePanel.GetComponent<RectTransform>().DOAnchorPosX(0, .33f).SetEase(Ease.InQuad);
			}
		}

		private void OnOpenGeodeDropdown()
		{
			if (geodeDropdownPanel.IsExpanded)
			{
				geodeDropdownPanel.GetComponent<RectTransform>().DOAnchorPosY(0f, .33f).SetEase(Ease.OutQuad);
			}
			else
			{
				geodeDropdownPanel.GetComponent<RectTransform>().DOAnchorPosY(-330f, .33f).SetEase(Ease.InQuad);
			}
		}

		public void OnPlayMatchPressed(Tuple<GrubbitEnums.MatchmakingMode, GrubbitEnums.GrubbitGameFormat> gameSettings)
		{
			var matchmakingMode = gameSettings.Item1;
			var format = gameSettings.Item2;
			SceneLoader.Instance.Load(SceneLoader.Scene.MatchmakingScene, false);
		}

		private void OnChangeDeckBtnPressed()
		{

		}

		public void PromptApplicationQuit()
		{

		}
	}
}



