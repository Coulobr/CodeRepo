using System;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Grubbit
{
	public class GrubbitCardTemplate : MonoBehaviour
	{
		[HideInInspector]
		public CardData cardData;

		[Header("Rarity Icons")]
		public Sprite rarityCommon;
		public Sprite rarityLegendary;
		public Image cardSplashImage;
		public Image cardRarityIcon;

		[Header("Card Text")]
		public TextMeshProUGUI cardNameText;
		public TextMeshProUGUI cardTypeText;
		public TextMeshProUGUI cardDescriptionText;
		public TextMeshProUGUI cardFlavorText;

		[Header("Stats Containers")]
		public RectTransform grubContainer;
		public RectTransform armorContainer;
		public RectTransform healthContainer;
		public RectTransform powerContainer;

		[Header("Stats Text")]
		public TextMeshProUGUI cardPowerText;
		public TextMeshProUGUI cardHealthText;
		public TextMeshProUGUI cardGrubText;
		public TextMeshProUGUI cardArmorText;

		[ContextMenu("Apply Card Data")]
		private void DebugSetCardData()
		{
			if (cardData == null)
			{
				Debug.LogError("CardData isn't assigned in the Inspector");
				return;
			}
			cardSplashImage.sprite = cardData.img;
			cardRarityIcon.sprite = cardData.subType == GrubbitEnums.CardSubType.Legendary ? rarityLegendary : rarityCommon;
			cardNameText.text = cardData.cardName;
			cardTypeText.text = $"{GrubbitEnums.CardTypeToString(cardData.type)} - {GrubbitEnums.CardSubtypeToString(cardData.subType)}";
			cardDescriptionText.text = cardData.description;
			cardFlavorText.text = cardData.flavorText;

			cardPowerText.text = cardData.power > 0 ? cardData.power.ToString() : "";
			cardHealthText.text = cardData.health > 0 ? cardData.health.ToString() : "";
			cardGrubText.text = cardData.cost > 0 ? cardData.cost.ToString() : "";
			cardArmorText.text = cardData.armor > 0 ? cardData.armor.ToString() : "";

			healthContainer.gameObject.SetActive(cardData.type == GrubbitEnums.CardType.ToadCard);
			powerContainer.gameObject.SetActive(cardData.type == GrubbitEnums.CardType.ToadCard);
			armorContainer.gameObject.SetActive(cardData.type == GrubbitEnums.CardType.Trinket);
			grubContainer.gameObject.SetActive(cardData.type is
				GrubbitEnums.CardType.Activity
				or GrubbitEnums.CardType.BreakEvent
				or GrubbitEnums.CardType.Trinket);
		}

		public void InitializeCardById(string cardId)
		{
			GrubbitCardUtility.GetOrLoadCardDataById(cardId, SetCardData);
		}

		public void SetCardData(CardData data)
		{
			if (data == null) return;
			cardData = data;

			cardSplashImage.sprite = data.img;
			cardRarityIcon.sprite = data.subType == GrubbitEnums.CardSubType.Legendary ? rarityLegendary : rarityCommon;
			cardNameText.text = data.cardName;
			cardTypeText.text = $"{GrubbitEnums.CardTypeToString(data.type)} - {GrubbitEnums.CardSubtypeToString(data.subType)}";
			cardDescriptionText.text = data.description;
			cardFlavorText.text = data.flavorText;

			cardPowerText.text = data.power == -2 ? "?" : data.power.ToString();
			cardHealthText.text = data.health > 0 ? data.health.ToString() : "";
			cardGrubText.text = data.cost > 0 ? data.cost.ToString() : "";
			cardArmorText.text = data.armor == -1 ? "∞" : data.armor.ToString();

			healthContainer.gameObject.SetActive(data.type == GrubbitEnums.CardType.ToadCard);
			powerContainer.gameObject.SetActive(data.type == GrubbitEnums.CardType.ToadCard);
			armorContainer.gameObject.SetActive(data.type == GrubbitEnums.CardType.Trinket);
			grubContainer.gameObject.SetActive(data.type is 
				GrubbitEnums.CardType.Activity 
				or GrubbitEnums.CardType.BreakEvent 
				or GrubbitEnums.CardType.Trinket);

			var cardScript = GrubbitCardUtility.GetCardTypeById(data.cardId);
			if (cardScript != null)
			{
				gameObject.AddComponent(cardScript.GetType());
			}
		}
	}
}
