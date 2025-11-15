using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grubbit
{
	public class GeodeDropdownPanel : Panel<GeodeDropdownPanel>
	{
		public bool IsExpanded => GetComponent<RectTransform>().anchoredPosition.y != 0;
		[Header("Img/Sprites")]
		public Image geodeImg;

		[Header("Text")]
		public TextMeshProUGUI geodeCount;

		[Header("Buttons")]
		public SmartButton geodeRightButton;
		public SmartButton geodeLeftButton;
		public SmartButton openButton;
		public SmartButton plusButton;

		protected override void OnOpened()
		{
			base.OnOpened();
			SetDefaultState();
			SetDefaultPosition();
			SubscribeToButtonEvents();
		}

		private void SetDefaultState()
		{
			transform.SetSiblingIndex(0);
		}

		private void SetDefaultPosition()
		{
			var rect = GetComponent<RectTransform>();
			rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
		}

		private void SubscribeToButtonEvents()
		{
			UnsubscribeToButtonEvents();
			geodeRightButton.onClick.AddListener(OnRightArrowClick);
			geodeLeftButton.onClick.AddListener(OnLeftArrowClick);
			plusButton.onClick.AddListener(OnPlusButtonClick);
			openButton.onEndHold.AddListener(Open_OnPointerUp);
		}

		private void UnsubscribeToButtonEvents()
		{
			geodeRightButton.onClick.RemoveAllListeners();
			geodeLeftButton.onClick.RemoveAllListeners();
			plusButton.onClick.RemoveAllListeners();
			openButton.onEndHold.RemoveAllListeners();
		}

		private void OnRightArrowClick()
		{

		}

		private void OnLeftArrowClick()
		{

		}

		private void OnPlusButtonClick()
		{

		}

		private void Open_OnPointerUp()
		{
			if (!openButton.hovering)
			{
				return;
			}

			OpenGeode();
		}

		private void OpenGeode()
		{
		}
	}
}