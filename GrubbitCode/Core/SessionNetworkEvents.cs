using System;
using System.Collections.Generic;
using Grubbit.Server;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Grubbit
{
	public class SessionNetworkEvents : MonoBehaviour
	{
		public bool IsServer(GameSession session)
		{
			return session.IsServer;
		}

		public bool IsClient(GameSession session)
		{
			return session.IsClient;
		}


		public bool IsMine(GameSession session)
		{
			return session.IsLocalPlayer;
		}

		[Rpc(SendTo.Server)]
		public void AddCardToStackServerRpc(string cardId, string playerIdOwner)
		{
			if (IsServer(out var session))
			{
				session.ProcessClientRequest_AddCardToStack(cardId, playerIdOwner);
			}
		}

		[Rpc(SendTo.Server)]
		public void PassPlayerPriorityServerRpc(string playerIdOwner)
		{
			if (IsServer(out var session))
			{
				session.ProcessClientRequest_PassPlayerPriority(playerIdOwner);
			}
		}

		[Rpc(SendTo.Server)]
		public void PlayerUseTrinketServerRpc(string playerIdOwner)
		{
			if (IsServer(out var session))
			{
				session.ProcessClientRequest_UseTrinket(playerIdOwner);
			}
		}

		[Rpc(SendTo.Server)]
		public void MultipleChoiceSelectServerRpc(int choiceSelected, string cardId, string playerIdWhoSelected)
		{
			if (IsServer(out var session))
			{
				session.ProcessClientRequest_MultipleChoiceSelect(choiceSelected, cardId, playerIdWhoSelected);
			}
		}

		[Rpc(SendTo.Server)]
		public void PlayerSelectDiscardCardFromHandServerRpc(string cardId, string playerIdWhoSelected)
		{
			if (IsServer(out var session))
			{
				session.ProcessClientRequest_PlayerDiscardCardFromHand(cardId, playerIdWhoSelected);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void AddAuraToPlayerClientRpc(string playerIdRecipient, GrubbitEnums.GrubbitAuras appliedAura)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerAddAura(playerIdRecipient, appliedAura);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void CardStatsAdjustedClientRpc(string cardId, string cardStatsJsonObject)
		{
			if (IsClient(out var session))
			{
				session.OnCardStatsAdjusted(cardId, cardStatsJsonObject);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void RemoveAurasFromPlayerClientRpc(string desiredPlayerId, List<GrubbitEnums.GrubbitAuras> aurasToRemove)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerRemoveAuras(desiredPlayerId, aurasToRemove);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void RemoveAuraFromPlayerClientRpc(string desiredPlayerId, GrubbitEnums.GrubbitAuras auraToRemove)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerRemoveAura(desiredPlayerId, auraToRemove);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void DiscardCardClientRpc(string cardId, string playerIdOwner)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDiscardCard(cardId, playerIdOwner);
			}
		}

		[Rpc(SendTo.SpecifiedInParams, AllowTargetOverride = true)]
		public void ChoiceDiscardFromHandClientRpc(List<string> cardsInHand, string playerIdToDiscard, RpcParams rpcParams)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDiscardCardSelect(cardsInHand, playerIdToDiscard);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void ExileEntireDiscardPileClientRpc(string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerExileEntireDiscardPile(playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void DestroyCardsClientRpc(List<string> cardIds, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDestroyCards(cardIds, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void DestroyCardClientRpc(string cardId, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDestroyCard(cardId, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void DestroyTrinketClientRpc(string cardId, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDestroyTrinket(cardId, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void DestroyWeaponClientRpc(string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDestroyWeapon(playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void ExileCardClientRpc(string cardId, string playerIdOwner)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerExileCard(cardId, playerIdOwner);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void DealDamageClientRpc(int damageDealt, string cardId, string damageDealerPlayerId, string damageRecipientPlayerId)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDealDamage(damageDealt, cardId, damageDealerPlayerId, damageRecipientPlayerId);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void DrainHealthClientRpc(int healthDrained, string cardId, string damageDealerPlayerId, string damageRecipientPlayerId)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDrainHealth(healthDrained, cardId, damageDealerPlayerId, damageRecipientPlayerId);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void AddGrubClientRpc(int grubAdded, int resultingGrub, string cardId, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerAddGrub(grubAdded, resultingGrub, cardId, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void AddHeroPermanentPowerClientRpc(int powerAdded, int resultingPower, string cardId, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerAddPermanentHeroPower(powerAdded, resultingPower, cardId, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void AddArmorClientRpc(int armorAdded, int resultingArmor, string cardId, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerAddArmor(armorAdded, resultingArmor, cardId, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void UpdateTrinketUsesClientRpc(int usesRemaining, string trinketOwnerId)
		{
			if (IsClient(out var session))
			{
				session.OnUpdatePlayerTrinketUses(usesRemaining, trinketOwnerId);
			}
		}

		[Rpc(SendTo.SpecifiedInParams, AllowTargetOverride = true)]
		public void DrawCardsClientRpc(List<string> cardIdsDrawn, string playerIdToDraw, RpcParams rpcParams)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerDrawCards(cardIdsDrawn, playerIdToDraw);
			}
		}

		[Rpc(SendTo.SpecifiedInParams, AllowTargetOverride = true)]
		public void OnOpponentDrawCardsClientRpc(int numberOfCardsDrawn, RpcParams rpcParams)
		{
			if (IsClient(out var session))
			{
				session.OnOpponentDrawCards(numberOfCardsDrawn);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void EquipWeaponClientRpc(int power, int numUses, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerEquipWeapon(power, numUses, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void PlayerWeaponStatsAdjustmentClientRpc(int resultingPower, int resultingUses, int powerAdjustment, int usesAdjustment, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerWeaponStatsAdjustment(resultingPower, resultingUses, powerAdjustment, usesAdjustment, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void EquipTrinketClientRpc(string cardId, int numUses, string playerIdRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerEquipTrinket(cardId, numUses, playerIdRecipient);
			}
		}

		[Rpc(SendTo.SpecifiedInParams, AllowTargetOverride = true)]
		public void MultipleChoiceClientRpc(List<string> choices, string cardId, string playerIdCaster, string playerIdRecipient, RpcParams rpcParams)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerMultipleChoice(choices, cardId, playerIdCaster, playerIdRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void ResolveCardClientRpc(string cardId, string playerIdOwner, string playerIdRewardRecipient)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerResolvedCard(cardId, playerIdOwner, playerIdRewardRecipient);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void RevealEventCardClientRpc(string cardIdRevealed, string playerIdOwner)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerEventCardRevealed(cardIdRevealed, playerIdOwner);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void ToadCombatResultClientRpc(string cardCombated, string playerIdOwner, string playerIdCombated, string playerIdRewardRecipient, bool result, double numHits, int playerArmor, int playerHealth)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerToadCombatResult(cardCombated, playerIdOwner, playerIdCombated, playerIdRewardRecipient, result, numHits, playerArmor, playerHealth);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void AddCardToStackClientRpc(string cardId, string playerIdOwner)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerAddCardToStack(cardId, playerIdOwner);
			}
		}

		[Rpc(SendTo.NotServer)]
		public void PassPriorityClientRpc(string playerIdOwner)
		{
			if (IsClient(out var session))
			{
				session.OnPlayerPassPriority(playerIdOwner);
			}
		}
	}
}