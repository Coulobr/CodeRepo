using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Grubbit
{
	public class InGameClientSession : GrubbitSession
	{

		/// <summary>
		/// Called by the local client to issue a request to play a card
		/// </summary>
		/// <param name="cardId"> The card to be played </param>
		/// <param name="playerIdOwner"> The owner of the card </param>
		public void PlayCardRequest(string cardId, string playerIdRequesting)
		{
			GrubbitGameManager.Instance.SessionNetworkEvents.AddCardToStackServerRpc(cardId, playerIdRequesting);
		}

		/// <summary>
		/// Called by the local client to issue a request to pass priority
		/// </summary>
		public void PlayerPassTurnRequest(string playerIdRequesting)
		{
			GrubbitGameManager.Instance.SessionNetworkEvents.PassPlayerPriorityServerRpc(playerIdRequesting);
		}

		/// <summary>
		/// Called by the local client to issue a request to use their equipped trinket
		/// </summary>
		public void PlayerUseTrinketRequest(string playerIdRequesting)
		{
			GrubbitGameManager.Instance.SessionNetworkEvents.PlayerUseTrinketServerRpc(playerIdRequesting);
		}

		/// <summary>
		/// Called when the server resolves any players "Pass Priority" request
		/// </summary>
		public void OnPlayerPassPriority(string playerIdWhoPassed)
		{

		}

		/// <summary>
		/// Called when the server resolves a discard card event
		/// </summary>
		public void OnPlayerDiscardCard(string cardId, string playerIdWhoDiscarded)
		{

		}

		/// <summary>
		/// Called when the server resolves multiple destroy card events simultaneously
		/// </summary>
		public void OnPlayerDestroyCards(List<string> cardIds, string playerIdOwnerOfCards)
		{

		}

		/// <summary>
		/// Called when the server resolves a destroy card event
		/// </summary>
		public void OnPlayerDestroyCard(string cardId, string playerIdOwnerOfCard)
		{

		}

		public void OnPlayerDestroyTrinket(string cardId, string playerIdOwnerOfCard)
		{

		}

		public void OnUpdatePlayerTrinketUses(int usesRemaining, string trinketOwnerPlayerId)
		{

		}

		public void OnPlayerDestroyWeapon(string playerIdOwnerOfWeapon)
		{

		}

		/// <summary>
		/// Called when the server resolves an exile card event
		/// </summary>
		public void OnPlayerExileCard(string cardId, string playerIdWhoExiled)
		{

		}

		/// <summary>
		/// Called when the server resolves a deal damageDealt event
		/// </summary>
		public void OnPlayerDealDamage(int damageDealt, string cardId, string playerIdCaster, string playerIdRecipient)
		{

		}

		/// <summary>
		/// Called when the server resolves a deal damage event
		/// </summary>
		public void OnPlayerDrainHealth(int healthDrained, string cardId, string playerIdCaster, string playerIdRecipient)
		{

		}

		/// <summary>
		/// Called when the server adds grub for a player
		/// </summary>
		public void OnPlayerAddGrub(int grubAdded, int resultingGrub, string cardId, string playerWhoReceivedGrub)
		{

		}

		/// <summary>
		/// Called when the server changes the power of a players hero
		/// </summary>
		public void OnPlayerAddPermanentHeroPower(int powerAdded, int resultingPower, string cardId, string playerIdRecipient)
		{

		}

		/// <summary>
		/// Called when the server adds grub armor to a player
		/// </summary>
		public void OnPlayerAddArmor(int armorAdded, int resultingArmor, string cardId, string playerWhoReceivedArmor)
		{

		}

		/// <summary>
		/// Called when the server adds grub armor to a player
		/// </summary>
		public void OnPlayerAddAura(string playerIdRecipient, GrubbitEnums.GrubbitAuras appliedAura)
		{

		}

		/// <summary>
		/// Called when the server adds grub armor to a player
		/// </summary>
		public void OnCardStatsAdjusted(string cardId, string cardStatsJsonObject)
		{
			CardStats newCardStats = JsonUtility.FromJson<CardStats>(cardStatsJsonObject);
		}

		/// <summary>
		/// Called when the server adds grub armor to a player
		/// </summary>
		public void OnPlayerRemoveAuras(string desiredPlayerId, List<GrubbitEnums.GrubbitAuras> aurasToRemove)
		{

		}

		/// <summary>
		/// Called when the server adds grub armor to a player
		/// </summary>
		public void OnPlayerRemoveAura(string desiredPlayerId, GrubbitEnums.GrubbitAuras auraToRemove)
		{

		}

		/// <summary>
		/// Called when the server resolves a draw card event
		/// </summary>
		public void OnPlayerDrawCards(List<string> cardIdsDrawn, string playerWhoDrewCards)
		{

		}

		/// <summary>
		/// Called when the server resolves a draw card event
		/// </summary>
		public void OnOpponentDrawCards(int numberOfCardsDrawn)
		{

		}

		/// <summary>
		/// Called when the server resolves a draw card event
		/// </summary>
		public void OnPlayerEquipWeapon(int power, int numUses, string playerWhoEquippedWeapon)
		{

		}

		/// <summary>
		/// Called when the server resolves a draw card event
		/// </summary>
		public void OnPlayerWeaponStatsAdjustment(int resultingPower, int resultingUses, int powerAdjustment, int usesAdjustment, string playerIdRecipient)
		{

		}

		public void OnPlayerEquipTrinket(string cardId, int numUses, string playerWhoEquippedTrinket)
		{

		}

		public void OnPlayerMultipleChoice(List<string> choices, string cardId, string playerIdCaster, string playerIdRecipient)
		{
			if (!GrubbitGameManager.Instance.SessionNetworkEvents.IsMine(playerIdRecipient)) return;

			// TODO: Open Popup with callbacks

			// Example: Selected the first choice
			GrubbitGameManager.Instance.SessionNetworkEvents.MultipleChoiceSelectServerRpc(0, cardId, playerIdRecipient);
		}

		public void OnPlayerDiscardCardSelect(List<string> cardsInHand, string playerIdToDiscard)
		{
			if (!GrubbitGameManager.Instance.SessionNetworkEvents.IsMine(playerIdToDiscard)) return;

			// TODO: Open Popup with callbacks

			// Example: Selected the first choice
			GrubbitGameManager.Instance.SessionNetworkEvents.PlayerSelectDiscardCardFromHandServerRpc(cardsInHand[0], playerIdToDiscard);
		}

		/// <summary>
		/// Called when the server resolves a request to add a card to the call stack to be resolved when necessary
		/// </summary>
		public void OnPlayerExileEntireDiscardPile(string playerWhoExiled)
		{

		}

		/// <summary>
		/// Called when the server resolves a request to add a card to the call stack to be resolved when necessary
		/// </summary>
		public void OnPlayerAddCardToStack(string cardId, string playerIdCaster)
		{

		}

		/// <summary>
		/// Called when the server resolves a cards unique behavior
		/// </summary>
		public void OnPlayerResolvedCard(string cardId, string playerIdCaster, string playerIdRewardRecipient)
		{

		}

		/// <summary>
		/// Called when the server reveals an event card 
		/// </summary>
		public void OnPlayerEventCardRevealed(string cardIdRevealed, string playerIdOwner)
		{

		}

		/// <summary>
		/// Called when the server resolves a combat phase
		/// </summary>
		public void OnPlayerToadCombatResult(string cardCombated, string playerIdOwner, string playerIdCombated, string playerIdRewardRecipient, bool result, double numHits, int playerArmor, int playerHealth)
		{

		}
	}
}


