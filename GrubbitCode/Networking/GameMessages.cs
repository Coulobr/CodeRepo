// ==============================================
 // Grubbit Net-Shared: Events & Payloads (v2)
 // ==============================================
 using System;
 using UnityEngine;

 namespace Grubbit.NetShared
 {
	// ==== NEW S2C events (server -> client) ====
	public enum S2CEvent
	{
        AcceptPrompt = 0,
        DrawCardsSelf = 1,
        DrawCardsOpponent = 2,
        TurnChanged = 3,
        TurnEnded = 4,
        CardPlayedSelf = 5,
        CardPlayedOpponent = 6,
        CardDiscardedSelf = 7,
        CardDiscardedOpponent = 8,
        CardExiledSelf = 9,
        CardExiledOpponent = 10,
        DamageApplied = 11,
        HealApplied = 12,
        StatusApplied = 13,
        MatchCanceled = 14,
        Error = 15,
		// ... keep existing ...
		StackItemAdded = 60,
		StackUpdated = 61,
		StackResolved = 62,
		ChoiceRequested = 63,
		ChoiceResolved = 64,
		PlayerConceded = 65,
		EmoteReceived = 66,
		Pong = 67,

		AuraAdded = 70,
		AuraRemoved = 71,
		AurasRemoved = 72,

		CardStatsAdjusted = 75,

		DiscardPerformed = 80,      // (legacy mirror in addition to CardDiscarded*)
		ExilePerformed = 81,        // (legacy mirror in addition to CardExiled*)
		ExileEntireDiscard = 82,

		CardsDestroyed = 85,
		CardDestroyed = 86,
		TrinketDestroyed = 87,
		WeaponDestroyed = 88,

		EquipWeapon = 90,
		WeaponStatsAdjusted = 91,
		EquipTrinket = 92,

		EventCardRevealed = 95,
		CardResolved = 96,

		ToadCombatResult = 100,

		PriorityPassed = 110,

		GrubChanged = 120,
		PowerChanged = 121,
		ArmorChanged = 122,
		TrinketUsesUpdated = 123,
	}

	// ==== NEW C2S events (client -> server) ====
	public enum C2SEvent
	{
		// ... keep existing ...
		AddCardToStack = 14,
		ChoiceResponse = 15,
		Concede = 16,
		Emote = 17,
		Ping = 18,

		PassPriority = 19,
		UseTrinket = 20,

		// convenience requests (if you want to keep these entry points)
		PlayerDiscardCardFromHand = 21,
		MultipleChoiceSelect = 22
	}

	// ==== Payloads you don't already have ====

	// ----- Auras -----
	[Serializable] public class AuraAddedPayload { public ulong playerClientId; public string auraId; }
	[Serializable] public class AuraRemovedPayload { public ulong playerClientId; public string auraId; }
	[Serializable] public class AurasRemovedPayload { public ulong playerClientId; public string[] auraIds; }

	// ----- Card stats adjust -----
	[Serializable] public class CardStatsAdjustedPayload { public string cardId; public string statsJson; }

	// ----- Discard / Exile -----
	[Serializable] public class DiscardPerformedPayload { public ulong actorClientId; public string cardId; }
	[Serializable] public class ExilePerformedPayload { public ulong actorClientId; public string cardId; }
	[Serializable] public class ExileEntireDiscardPayload { public ulong playerClientId; }

	// ----- Destroy -----
	[Serializable] public class CardsDestroyedPayload { public ulong playerClientId; public string[] cardIds; }
	[Serializable] public class CardDestroyedPayload { public ulong playerClientId; public string cardId; }
	[Serializable] public class TrinketDestroyedPayload { public ulong playerClientId; public string cardId; } // cardId optional
	[Serializable] public class WeaponDestroyedPayload { public ulong playerClientId; }

	// ----- Equip & weapon/trinket updates -----
	[Serializable] public class EquipWeaponPayload { public ulong playerClientId; public int power; public int uses; }
	[Serializable] public class WeaponStatsAdjustedPayload { public ulong playerClientId; public int resultingPower; public int resultingUses; public int powerAdjustment; public int usesAdjustment; }
	[Serializable] public class EquipTrinketPayload { public ulong playerClientId; public string cardId; public int uses; }

	// ----- Reveal / Resolve -----
	[Serializable] public class EventCardRevealedPayload { public ulong ownerClientId; public string cardId; }
	[Serializable] public class CardResolvedPayload { public ulong ownerClientId; public ulong rewardRecipientClientId; public string cardId; }

	// ----- Toad Combat -----
	[Serializable]
	public class ToadCombatResultPayload
	{
		public string cardId; public ulong ownerClientId;
		public ulong combatedClientId; public ulong rewardRecipientClientId;
		public bool result; public double numHits; public int playerArmor; public int playerHealth;
	}

	// ----- Priority -----
	[Serializable] public class PriorityPassedPayload { public ulong playerClientId; }

	// ----- Resource/Stat deltas -----
	[Serializable] public class GrubChangedPayload { public ulong playerClientId; public int added; public int resulting; public string sourceCardId; }
	[Serializable] public class PowerChangedPayload { public ulong playerClientId; public int added; public int resulting; public string sourceCardId; }
	[Serializable] public class ArmorChangedPayload { public ulong playerClientId; public int added; public int resulting; public string sourceCardId; }
	[Serializable] public class TrinketUsesUpdatedPayload { public ulong playerClientId; public int usesRemaining; }

	// ----- Stack & choices (already added earlier) -----
	[Serializable] public class AddCardToStackRequest { public string cardId; public string actionId; public ulong[] targetClientIds; public string[] targetCardIds; public bool revealToOpponent = true; }
	[Serializable] public class StackItemPayload { public int stackItemId; public string sourceCardId; public ulong sourceClientId; public string actionId; public ulong[] targetClientIds; public string[] targetCardIds; public bool revealedToOpponent; }
	[Serializable] public class StackStatePayload { public StackItemPayload[] items; }

	[Serializable]
	public class ChoiceRequestPayload
	{
		public int requestId; public ulong recipientClientId; public string choiceType; public string prompt; public int min; public int max;
		public string[] candidateCardIds; public ulong[] candidateClientIds; public int timeoutSeconds;
	}
	[Serializable] public class ChoiceResponseRequest { public int requestId; public string[] chosenCardIds; public ulong[] chosenClientIds; }

	// Convenience requests if you want to keep the same names:
	[Serializable] public class MultipleChoiceSelectRequest { public int requestId; public int choiceSelected; public string cardId; }
	[Serializable] public class DiscardFromHandRequest { public string cardId; }
	[Serializable] public class UseTrinketRequest { public string cardId; }  // optional; can be null if only one equipped


	// -------- Ready check --------
	[Serializable]
     public class AcceptPromptPayload
     {
         public string matchId;
         public int acceptWindowSeconds;
     }

     [Serializable]
     public class AcceptResponsePayload
     {
         public string matchId;
         public bool accept;
     }

     [Serializable]
     public class AcknowledgePayload
     {
         public int eventId;
     }
     // -------- Draws --------
     [Serializable]
     public class DrawCardsSelfPayload
     {
         public ulong recipientClientId;
         public string[] cardIds;
         public int count;
     }

     [Serializable]
     public class DrawCardsOpponentPayload
     {
         public ulong recipientClientId;
         public int count;
     }
     // -------- Turns --------
     [Serializable]
     public class TurnChangedPayload
     {
         public ulong currentTurnClientId;
         public int turnNumber;
     }

     [Serializable]
     public class TurnEndedPayload
     {
         public ulong previousTurnClientId;
         public int turnNumber;
     }
     // -------- Card requests (C2S) --------
     [Serializable]
     public class PlayCardRequest
     {
         public string cardId;
         public ulong[] targetClientIds;
         public string[] targetCardIds;
     }

     [Serializable]
     public class DiscardCardRequest
     {
         public string cardId;
         public bool revealToOpponent;
     }

     [Serializable]
     public class ExileCardRequest
     {
         public string cardId;
         public bool revealToOpponent;
     }
     // -------- Card result notifications (S2C) --------
     [Serializable]
     public class CardPlayedSelfPayload
     {
         public ulong actorClientId;
         public string cardId;
     }

     [Serializable]
     public class CardPlayedOpponentPayload
     {
         public ulong actorClientId;
         public string cardId;
     }

     [Serializable]
     public class CardMovedPayload
     {
         public ulong actorClientId;
         public string cardId;
         public bool revealed;
     }
     // -------- Effects --------
     [Serializable]
     public class DamageAppliedPayload
     {
         public ulong targetClientId;
         public int amount;
         public string sourceCardId;
         public int resultingHealth;
         public int resultingArmor;
     }

     [Serializable]
     public class HealAppliedPayload
     {
         public ulong targetClientId;
         public int amount;
         public string sourceCardId;
         public int resultingHealth;
     }

     [Serializable]
     public class StatusAppliedPayload
     {
         public ulong targetClientId;
         public string statusId;
         public int stacks;
         public int durationTurns;
         public string sourceCardId;
     }

     [Serializable]
     public class PlayerConcededPayload
     {
	     public ulong playerClientId;
     }

     [Serializable]
     public class EmotePayload
     {
	     public ulong fromClientId;
	     public string emoteId; // e.g., "wave", "thanks"
     }

     [Serializable]
     public class PongPayload
     {
	     public long clientTimeSent; // echo
     }
	public static class NetJson
     {
         public static string ToJson<T>(T obj) => JsonUtility.ToJson(obj);
         public static T FromJson<T>(string json) => JsonUtility.FromJson<T>(json);
     }
 }

