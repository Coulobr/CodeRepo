using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Grubbit.Shared
{
public class PlayerGameStateData
{
		public int health = 0;
		public int armor = 0;
		public int grub = 0;
		public int heroPower;
		public bool isWeaponEquipped = false;
		public int weaponPower = 0;
		public int weaponDurability = 0;
		public bool isTrinketEquipped = false;
		public int trinketDurability = 0;
		public string trinketCardId;
		public List<string> discardPile = new();
		public List<string> exilePile = new();
		public List<string> cardsInHand = new();
		public List<string> cardsInDeck = new();
		public List<string> cardsInPlay = new();
		public List<string> cardsInMatchPlot = new();
		public List<string> cardsPlayed = new();
		public Dictionary<GrubbitEnums.GrubbitAuras, int> activeAurasList = new(); // Buff/Debuff, # applications remaining
		public string lastPlayedCardId = "";
	


	/// <summary>
	/// Marks the current session as started, so from now on we keep the data of disconnected players.
	/// </summary>
	public override void LoadSession()
	{
		base.LoadSession();
		playerIdToOnlineData = new Dictionary<string, OnlinePlayerSessionData>();
		clientIdToPlayerId = new Dictionary<ulong, string>();
		playerIdToGameStateData = new Dictionary<string, PlayerGameStateData>();
		cardCallStack = new List<CardBase>();
	}

	/// <summary>
	/// Reinitialize session data from connected players, and clears data from disconnected players, so that if they reconnect in the next game, they will be treated as new players
	/// </summary>
	public override void UnloadSession()
	{
		base.UnloadSession(); 
		playerIdToOnlineData.Clear();
		clientIdToPlayerId.Clear();
		cardCallStack.Clear();
		playerIdToGameStateData.Clear();
		ClearDisconnectedPlayersData();
		ReinitializePlayersData();
	}

	/// <summary>
	/// Handles client disconnect."
	/// </summary>
	public void DisconnectClient(ulong clientId)
	{
		if (SessionActive)
		{
			// Mark client as disconnected, but keep their data so they can reconnect.
			if (clientIdToPlayerId.TryGetValue(clientId, out var playerId))
			{
				if (GetPlayerData(playerId)?.clientId == clientId)
				{
					var data = this.playerIdToOnlineData[playerId];
					data.isConnected = false;
					this.playerIdToOnlineData[playerId] = data;
				}
			}
		}
		else
		{
			// Session has not started, no need to keep their data
			if (clientIdToPlayerId.Remove(clientId, out var playerId))
			{
				if (GetPlayerData(playerId)?.clientId == clientId)
				{
					playerIdToOnlineData.Remove(playerId);
				}
			}
		}
	}

	/// <summary>
	/// Returns the ClientRpcSendParams for the playerId which will *only* send the rpc to that client
	/// </summary>
	/// <param name="playerId"> The playerId to receive the RPC </param>
	/// <returns> The ClientRpcParams for the playerId </returns>
	private RpcParams GetClientRpcTarget(string playerId)
	{
		return !playerIdToOnlineData.TryGetValue(playerId, out var onlineData) ? null : NetworkManager.Singleton.RpcTarget.Single(onlineData.clientId, RpcTargetUse.Temp);
	}


	/// <summary>
	/// Adds a connecting player's session data if it is a new connection, or updates their session data in case of a reconnection.
	/// </summary>
	/// <param name="clientId">This is the clientId that NetCode assigned us on login. It does not persist across multiple logins from the same client. </param>
	/// <param name="playerName">This is the player name acquired through the auth service (Steam, etc) </param>
	/// <param name="playerId">This is the playerId that is unique to this client and persists across multiple logins from the same client</param>
	/// <param name="sessionPlayerData">The player's initial data</param>
	public override void SetupConnectingPlayerSessionData(string playerName, ulong clientId, string playerId, OnlinePlayerSessionData sessionPlayerData)
	{ 
		base.SetupConnectingPlayerSessionData(playerName, clientId, playerId, sessionPlayerData);
		var isReconnecting = false;

		// Test for duplicate connection
		if (IsDuplicateConnection(playerId))
		{
			Utility.LogError($"Player ID {playerId} already exists. This is a duplicate connection. Rejecting this session data.");
			return;
		}

		// If another client exists with the same playerId
		if (playerIdToOnlineData.TryGetValue(playerId, out var value))
		{
			if (!value.isConnected)
			{
				// If this connecting client has the same player Id as a disconnected client, this is a reconnection.
				isReconnecting = true;
			}
		}

		// Reconnecting. Give data from old player to new player
		if (isReconnecting)
		{
			// Update player session data
			sessionPlayerData = playerIdToOnlineData[playerId];
			sessionPlayerData.clientId = clientId;
			sessionPlayerData.playerName = playerName;
			sessionPlayerData.isConnected = true;
		}

		//Populate our dictionaries with the SessionPlayerData
		clientIdToPlayerId[clientId] = playerId; 
		playerIdToOnlineData[playerId] = sessionPlayerData;

		if (!isReconnecting)
		{
			playerIdToGameStateData.Add(playerId, new PlayerGameStateData());
		}
	}

	/// <summary>
	/// Returns the Player ID matching the given client Id
	/// </summary>
	public string GetPlayerIdByClientId(ulong clientId)
	{
		if (clientIdToPlayerId.TryGetValue(clientId, out var playerId))
		{
			return playerId;
		}

		Utility.Log($"No client player ID found mapped to the given client ID: {clientId}");
		return null;
	}

	/// <summary>
	/// Returns the opponents Player ID
	/// </summary>
	public string GetOpponentPlayerId(string requestingPlayerId)
	{
		if (playerIdToOnlineData is { Count: > 0 })
		{
			foreach (var playerId in playerIdToOnlineData.Keys.Where(playerId => playerId != requestingPlayerId))
			{
				return playerId;
			}
		}

		Utility.Log($"No client player ID found mapped to the given player ID: {requestingPlayerId}");
		return null;
	}

	/// <summary>
	/// Returns online player data matching the given ID
	/// </summary>
	public OnlinePlayerSessionData? GetPlayerData(ulong clientId)
	{
		//First see if we have a playerId matching the clientID given.
		var playerId = GetPlayerIdByClientId(clientId);
		if (playerId != null)
		{
			return GetPlayerData(playerId);
		}

		Utility.Log($"No client player ID found mapped to the given client ID: {clientId}");
		return null;
	}

	/// <summary>
	/// Returns the online player data matching the given ID
	/// </summary>
	public OnlinePlayerSessionData? GetPlayerData(string playerId)
	{
		if (playerIdToOnlineData.TryGetValue(playerId, out var data))
		{
			return data;
		}

		Utility.Log($"No PlayerData of matching player ID found: {playerId}");
		return null;
	}

	/// <summary>
	/// Updates player data
	/// </summary>
	/// <param name="clientId"> id of the client whose data will be updated </param>
	/// <param name="sessionPlayerData"> new data to overwrite the old </param>
	public void SetPlayerData(ulong clientId, OnlinePlayerSessionData sessionPlayerData)
	{
		if (clientIdToPlayerId.TryGetValue(clientId, out string playerId))
		{
			playerIdToOnlineData[playerId] = sessionPlayerData;
		}
		else
		{
			Utility.LogError($"No client player ID found mapped to the given client ID: {clientId}");
		}
	}

	public PlayerGameStateData GetPlayerGameStateData(string playerId)
	{
		return playerIdToGameStateData.GetValueOrDefault(playerId);
	}

	public void ReinitializePlayersData()
	{
		foreach (var id in clientIdToPlayerId.Keys)
		{
			var playerId = clientIdToPlayerId[id];
			var sessionPlayerData = playerIdToOnlineData[playerId];
			sessionPlayerData.Reinitialize();
			playerIdToOnlineData[playerId] = sessionPlayerData;
		}
	}

	public void ClearDisconnectedPlayersData()
	{
		var idsToClear = new List<ulong>();
		foreach (var id in clientIdToPlayerId.Keys)
		{
			var data = GetPlayerData(id);
			if (data is { isConnected: false })
			{
				idsToClear.Add(id);
			}
		}

		foreach (var id in idsToClear)
		{
			var playerId = clientIdToPlayerId[id];
			if (GetPlayerData(playerId)?.clientId == id)
			{
				playerIdToOnlineData.Remove(playerId);
			}

			clientIdToPlayerId.Remove(id);
		}
	}

	/// <summary>
	/// Returns true if a player with this ID is already connected
	/// </summary>
	public bool IsDuplicateConnection(string playerId)
	{
		return playerIdToOnlineData.ContainsKey(playerId) && playerIdToOnlineData[playerId].isConnected;
	}

	public void ProcessClientRequest_AddCardToStack(string cardId, string playerIdOwner)
	{
		var cardObject = GrubbitCardUtility.GetCardTypeById(cardId) as CardBase;
		var cardData = GrubbitCardUtility.GetCardDataById(cardId);
		if (cardObject == null || cardData == null) return;

		cardObject.InitializeCard(playerIdOwner, cardId, cardData.armor, cardData.cost, cardData.health, cardData.power);
		
		//ApplyPreCombatAuras(cardObject, playerIdOwner);

		cardCallStack.Add(cardObject);
		GrubbitGameManager.Instance.SessionNetworkEvents.AddCardToStackClientRpc(cardId, playerIdOwner);
	}

	public void ProcessClientRequest_PassPlayerPriority(string playerIdOwner)
	{
		GrubbitGameManager.Instance.SessionNetworkEvents.PassPriorityClientRpc(playerIdOwner);
	}

	public void ProcessClientRequest_MultipleChoiceSelect(int choiceSelected, string cardId, string playerIdWhoSelected)
	{
		switch (cardId)
		{
			case "5":
				MultipleChoiceResolve_AWorthySacrifice(choiceSelected, cardId, playerIdWhoSelected);
				break;
		}
		pendingMultipleChoiceFromUser = false;
	}

	public void ProcessClientRequest_PlayerDiscardCardFromHand(string cardId, string playerIdOwner)
	{
		pendingMultipleChoiceFromUser = false;
		Server_DiscardCard(cardId, playerIdOwner);
	}

	public void ProcessClientRequest_UseTrinket(string playerIdRequesting)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRequesting, out var data)) return;
		if (!data.isTrinketEquipped || data.trinketDurability == 0) return;

		ProcessClientRequest_AddCardToStack(data.trinketCardId, playerIdRequesting);
	}

	public void Server_TryResolveCard(string cardId, string playerIdOwner, string playerIdToCombat, string playerIdRewardRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdOwner, out var cardOwnersData)) return;
		if (!playerIdToGameStateData.TryGetValue(playerIdOwner, out var playerRewardRecipientData)) return;

		var cardToPlay = GrubbitCardUtility.GetCardTypeById(cardId) as CardBase;
		if (cardToPlay == null) return;

		switch (GrubbitCardUtility.GetCardDataById(cardId).type)
		{
			case GrubbitEnums.CardType.Activity:
				if (cardToPlay.Resolve(playerIdOwner, playerIdRewardRecipient))
				{
					GrubbitGameManager.Instance.SessionNetworkEvents.ResolveCardClientRpc(cardId, playerIdOwner, playerIdRewardRecipient);
				}
				break;
			case GrubbitEnums.CardType.ToadCard:
				ApplyPreCombatAuras(cardToPlay, playerIdOwner);
				InitiateToadCardCombat(cardToPlay, cardId, playerIdOwner, playerIdToCombat, playerIdRewardRecipient,
					(result, numHits, playerArmor, playerHealth) =>
					{
						GrubbitGameManager.Instance.SessionNetworkEvents.ToadCombatResultClientRpc(cardId, playerIdOwner, playerIdToCombat, playerIdRewardRecipient, result, numHits, playerArmor, playerHealth);

						if (result) // If the player won and destroyed the card
						{
							if (cardToPlay.Resolve(playerIdOwner, playerIdRewardRecipient))
							{
								GrubbitGameManager.Instance.SessionNetworkEvents.ResolveCardClientRpc(cardId, playerIdOwner, playerIdRewardRecipient);
							}
						}
					});
				break;
			case GrubbitEnums.CardType.Trinket:

				break;
			default: break;
		}

		cardOwnersData.lastPlayedCardId = cardId;
		cardOwnersData.cardsInPlay.Add(cardToPlay);
		cardOwnersData.cardsPlayed.Add(cardId);
		allCardsPlayedThisMatchPlot.Add(cardToPlay);
	}

	private void InitiateToadCardCombat(CardBase card, string cardId, string playerIdOwner, string playerIdToFight, string playerIdRewardRecipient, Action<bool, double, int, int> onCombatComplete)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdOwner, out var cardOwnersData)) return;
		if (!playerIdToGameStateData.TryGetValue(playerIdRewardRecipient, out var playerRewardRecipientData)) return;

		card.CombatHitsTaken = 0;
		bool resultOfCombat = true;
		double numberOfHits; // 0.5 being hero attacked once and destroyed the card
		int playerHeroHealthRemaining;
		int playerHeroArmorRemaining;
		var playerAuras = GetPlayerGameStateData(playerIdToFight).activeAurasList;
		var aurasToRemove = new List<GrubbitEnums.GrubbitAuras>();

		foreach (var aura in playerAuras)
		{
			if (aura.Value == 0) continue;
			switch (aura.Key)
			{
				case GrubbitEnums.GrubbitAuras.AnnoyingFly:
					// TODO: -1 Power For Attack (3 uses)

					playerAuras[aura.Key] = Mathf.Clamp(aura.Value - 1, 0, 99);
					if (aura.Value == 0)
					{
						aurasToRemove.Add(aura.Key);
					}
					break;
			}
		}

		//TODO: Calculate combat
		//card.CombatHitsTaken++;

		// Post Combat Auras
		if (resultOfCombat && playerAuras.ContainsKey(GrubbitEnums.GrubbitAuras.TrinketBarrelOfBooze))
		{
			var usesRemaining = Server_UpdateTrinketUses(playerIdRewardRecipient, 1, GrubbitEnums.GrubbitAuras.TrinketBarrelOfBooze);
			if (usesRemaining != -1)
			{
				if (usesRemaining == 0)
				{
					aurasToRemove.Add(GrubbitEnums.GrubbitAuras.TrinketBarrelOfBooze);
				}

				Server_DrainHealth(2, cardId, playerIdRewardRecipient, GetOpponentPlayerId(playerIdRewardRecipient));
				StartCoroutine(Server_PlayerChoiceDiscardCard(1, playerIdRewardRecipient, GetOpponentPlayerId(playerIdRewardRecipient)));
			}
		}

		Server_RemoveAurasFromPlayer(playerIdToFight, aurasToRemove);
	}

	public int Server_UpdateTrinketUses(string playerIdRewardRecipient, int expendedUses, GrubbitEnums.GrubbitAuras trinket)
	{
		if (playerIdToGameStateData.TryGetValue(playerIdRewardRecipient, out var playerRewardRecipientData))
		{
			playerRewardRecipientData.activeAurasList[trinket] -= expendedUses;
			GrubbitGameManager.Instance.SessionNetworkEvents.UpdateTrinketUsesClientRpc(playerRewardRecipientData.activeAurasList[trinket], playerIdRewardRecipient);
			return playerRewardRecipientData.activeAurasList[trinket];
		}
		return -1;
	}

	public void Server_DiscardCard(string cardId, string playerIdOwner)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdOwner, out var data)) return;

		data.discardPile.Add(cardId);
		var card = data.cardsInPlay.Find(x => x.CardId == cardId);
		if (card != null)
		{
			data.cardsInPlay.Remove(card);
		}
		GrubbitGameManager.Instance.SessionNetworkEvents.DiscardCardClientRpc(cardId, playerIdOwner);
	}

	public void Server_DestroyCards(List<string> cardsToDestroy, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;

		data.cardsInMatchPlot.Clear();
		data.cardsInMatchPlot.AddRange(data.cardsInPlay);
		data.discardPile.AddRange(cardsToDestroy);

		GrubbitGameManager.Instance.SessionNetworkEvents.DestroyCardsClientRpc(cardsToDestroy, playerIdRecipient);
	}

	public void Server_DestroyCard(CardBase cardToDestroy, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;

		data.cardsInMatchPlot.Remove(cardToDestroy);
		data.discardPile.Add(cardToDestroy.CardId);

		GrubbitGameManager.Instance.SessionNetworkEvents.DestroyCardClientRpc(cardToDestroy.CardId, playerIdRecipient);
	}
	
	public void Server_DestroyTrinket(string cardId, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;

		data.trinketDurability = 0;
		data.isTrinketEquipped = false;
		data.discardPile.Add(cardId);

		GrubbitGameManager.Instance.SessionNetworkEvents.DestroyTrinketClientRpc(cardId, playerIdRecipient);
	}

	public void Server_DestroyWeapon(string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;

		data.weaponDurability = 0;
		data.weaponPower = 0;
		data.isWeaponEquipped = false;

		GrubbitGameManager.Instance.SessionNetworkEvents.DestroyWeaponClientRpc(playerIdRecipient);
	}

	public void Server_ExileCard(string cardId, string playerIdOwner)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdOwner, out var data)) return;

		data.exilePile.Add(cardId);
		var card = data.cardsInPlay.Find(x => x.CardId == cardId);
		if (card != null)
		{
			data.cardsInPlay.Remove(card);
		}
		GrubbitGameManager.Instance.SessionNetworkEvents.ExileCardClientRpc(cardId, playerIdOwner);
	}

	public void Server_RevealEventCard(string cardId, string playerIdOwner)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdOwner, out var data)) return;

		GrubbitGameManager.Instance.SessionNetworkEvents.RevealEventCardClientRpc(cardId, playerIdOwner);
	}

	public void Server_WipeMatchPlot()
	{
		foreach (var card in allCardsPlayedThisMatchPlot)
		{
			card.OnClearMatchPlot();
		}

		allCardsPlayedThisMatchPlot.Clear();
	}

	public bool Server_DealDamage(int damage, string cardId, string damageDealerPlayerId, string damageRecipientPlayerId)
	{
		if (!playerIdToGameStateData.TryGetValue(damageRecipientPlayerId, out var recipientsData)) return false;

		if (recipientsData.armor >= damage)
		{
			recipientsData.armor -= damage;
		}
		else if (recipientsData.armor != 0 && recipientsData.armor < damage)
		{
			var healthResult = Mathf.Clamp(recipientsData.health - (damage - recipientsData.armor), 0, 99);
			recipientsData.health = healthResult;
			recipientsData.armor = 0;
		}
		else
		{
			var healthResult = Mathf.Clamp(recipientsData.health - damage, 0, 99);
			recipientsData.health = healthResult;
		}

		GrubbitGameManager.Instance.SessionNetworkEvents.DealDamageClientRpc(damage, cardId, damageDealerPlayerId, damageRecipientPlayerId);
		return true;
	}

	public bool Server_DrainHealth(int damage, string cardId, string damageDealerPlayerId, string damageRecipientPlayerId)
	{
		if (!playerIdToGameStateData.TryGetValue(damageDealerPlayerId, out var castersData)) return false;
		if (!playerIdToGameStateData.TryGetValue(damageRecipientPlayerId, out var recipientsData)) return false;

		var healthToDrain = recipientsData.health >= damage ? damage : recipientsData.health;

		var recipientHealth = Mathf.Clamp(recipientsData.health - healthToDrain, 0, 99);
		recipientsData.health = recipientHealth;

		var casterHealthResult = Mathf.Clamp(castersData.health + healthToDrain, 0, 99);
		recipientsData.health = casterHealthResult;

		GrubbitGameManager.Instance.SessionNetworkEvents.DrainHealthClientRpc(healthToDrain, cardId, damageDealerPlayerId, damageRecipientPlayerId);
		return true;
	}

	public bool Server_AddGrub(int grubToAdd, string cardId, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return false;

		var resultingGrub = data.grub + grubToAdd;
		
		if (data.cardsInPlay.Find(x=>x.CardId == "7") != null) // 'Accountant' card id
		{
			resultingGrub += 1;
		}
			
		Math.Clamp(resultingGrub, 0, 99);
		GrubbitGameManager.Instance.SessionNetworkEvents.AddGrubClientRpc(grubToAdd, data.grub, cardId, playerIdRecipient);
		return true;

	}

	public bool Server_AddHeroPowerPermanent(int powerToAdd, string cardId, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return false;

		var resultingPower = data.heroPower + powerToAdd;
		Math.Clamp(resultingPower, 0, 99);
		GrubbitGameManager.Instance.SessionNetworkEvents.AddHeroPermanentPowerClientRpc(powerToAdd, data.heroPower, cardId, playerIdRecipient);
		return true;
	}

	public bool Server_AddArmor(int armorToAdd, string cardId, string armorRecipientPlayerId)
	{
		if (playerIdToGameStateData.TryGetValue(armorRecipientPlayerId, out var data))
		{
			var resultingArmor = data.armor + armorToAdd;
			Math.Clamp(resultingArmor, 0, 99);

			if (resultingArmor < data.armor && data.activeAurasList.ContainsKey(GrubbitEnums.GrubbitAuras.TrinketAcidCoatingSpray))
			{
				Server_RemoveAuraFromPlayer(armorRecipientPlayerId, GrubbitEnums.GrubbitAuras.TrinketAcidCoatingSpray);
				Server_DestroyTrinket(cardId, armorRecipientPlayerId);
				return true;
			}

			GrubbitGameManager.Instance.SessionNetworkEvents.AddArmorClientRpc(armorToAdd, data.armor, cardId, armorRecipientPlayerId);
			return true;
		}

		return false;
	}

	public void Server_DrawCards(int amount, string playerIdOwner)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdOwner, out var data)) return;
		if (data.cardsInDeck.Count == 0) return;

		var numberOfCardsToDraw = data.cardsInDeck.Count < amount ? data.cardsInDeck.Count : amount;
		var cardsIdsToDraw = data.cardsInDeck.GetRange(0, numberOfCardsToDraw);

		data.cardsInHand.AddRange(cardsIdsToDraw);
		data.cardsInDeck.RemoveRange(0, numberOfCardsToDraw);

		GrubbitGameManager.Instance.SessionNetworkEvents.DrawCardsClientRpc(cardsIdsToDraw, playerIdOwner, GetClientRpcTarget(playerIdOwner));
		GrubbitGameManager.Instance.SessionNetworkEvents.OnOpponentDrawCardsClientRpc(numberOfCardsToDraw, GetClientRpcTarget(GetOpponentPlayerId(playerIdOwner)));
	}

	public void Server_EquipWeapon(int power, int numUses, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;

		data.isWeaponEquipped = true;
		data.weaponPower = power;
		data.weaponDurability = numUses;

		GrubbitGameManager.Instance.SessionNetworkEvents.EquipWeaponClientRpc(power, numUses, playerIdRecipient);
	}

	public void Server_AdjustWeaponStats(int powerAdjustment, int usesAdjustment, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;
		if (!data.isWeaponEquipped) return;

		data.weaponPower += powerAdjustment;
		data.weaponDurability += powerAdjustment;
		Mathf.Clamp(data.weaponPower, 0, 99);
		Mathf.Clamp(data.weaponDurability, 0, 99);

		GrubbitGameManager.Instance.SessionNetworkEvents.PlayerWeaponStatsAdjustmentClientRpc(data.weaponPower, data.weaponDurability, powerAdjustment, usesAdjustment, playerIdRecipient);

		if (data.weaponDurability == 0)
		{
			Server_DestroyWeapon(playerIdRecipient);
		}
	}

	public void Server_EquipTrinket(string cardId, int numUses, string playerIdRecipient, GrubbitEnums.GrubbitAuras grubbitAura)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;

		data.isTrinketEquipped = true;
		data.trinketCardId = cardId;
		data.trinketDurability = numUses;

		Server_AddAuraToPlayer(playerIdRecipient, grubbitAura, numUses, false);
		GrubbitGameManager.Instance.SessionNetworkEvents.EquipTrinketClientRpc(cardId, numUses, playerIdRecipient);
	}

	public void Server_MultipleChoice(Action<string, string, string> cardFunction, string cardId, string playerIdCaster, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;
		pendingMultipleChoiceFromUser = true;
		cardFunction?.Invoke(cardId, playerIdCaster, playerIdRecipient);
	}

	public IEnumerator Server_PlayerChoiceDiscardCard(int numberOfCardsToDiscard, string playerIdCaster, string playerIdRecipient, Action onComplete = null)
	{
		if (playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data))
		{
			pendingMultipleChoiceFromUser = true;
			GrubbitGameManager.Instance.SessionNetworkEvents.ChoiceDiscardFromHandClientRpc(data.cardsInHand, playerIdRecipient, GetClientRpcTarget(playerIdRecipient));

			yield return new WaitUntil(() => pendingMultipleChoiceFromUser == false);
			onComplete?.Invoke();
		}
	}

	public void Server_ExileAllDiscardPile(string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;
		if (data.discardPile.Count == 0) return;

		data.exilePile.AddRange(data.discardPile);
		data.discardPile.Clear();

		GrubbitGameManager.Instance.SessionNetworkEvents.ExileEntireDiscardPileClientRpc(playerIdRecipient);
	}

	public void Server_AWorthySacrifice(string cardId, string playerIdCaster, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;

		var choices = new List<string>()
		{
			"Lose 1 Power Permanently",
			"Deal 4 Damage to your next Toad Card this Match Plot",
			"Lose 3 Power to destroy your next Toad Card this Match Plot"
		};

		GrubbitGameManager.Instance.SessionNetworkEvents.MultipleChoiceClientRpc(choices, cardId, playerIdCaster, playerIdRecipient, GetClientRpcTarget(playerIdRecipient));
	}

	private void MultipleChoiceResolve_AWorthySacrifice(int choice, string cardId, string playerIdRecipient)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdRecipient, out var data)) return;
		switch (choice)
		{
			case 0: // Lose 1 Power Permanently
				Mathf.Clamp(data.heroPower -= 1, 1, 99);
				GrubbitGameManager.Instance.SessionNetworkEvents.AddHeroPermanentPowerClientRpc(-1, data.heroPower, cardId, playerIdRecipient);
				break;
			case 1: // Deal 4 Damage to your next Toad Card this Match Plot
				Server_AddAuraToPlayer(playerIdRecipient, GrubbitEnums.GrubbitAuras.AWorthySacrificeDamage, 1);
				break;
			case 2: // Lose 3 Power to destroy your next Toad Card this Match Plot
				Mathf.Clamp(data.heroPower -= 3, 1, 99);
				GrubbitGameManager.Instance.SessionNetworkEvents.AddHeroPermanentPowerClientRpc(-3, data.heroPower, cardId, playerIdRecipient);
				Server_AddAuraToPlayer(playerIdRecipient, GrubbitEnums.GrubbitAuras.AWorthySacrificeDestroy, 1);
				break;
		}
	}

	public void Server_AddAuraToPlayer(string desiredPlayerId, GrubbitEnums.GrubbitAuras desiredAura, int numberOfApplications, bool fireNetworkEvent = false)
	{
		if (!playerIdToGameStateData.TryGetValue(desiredPlayerId, out var data)) return;
		if (!data.activeAurasList.TryAdd(desiredAura, numberOfApplications)) return;

		if (fireNetworkEvent)
		{
			GrubbitGameManager.Instance.SessionNetworkEvents.AddAuraToPlayerClientRpc(desiredPlayerId, desiredAura);
		}
	}

	public void Server_RemoveAurasFromPlayer(string desiredPlayerId, List<GrubbitEnums.GrubbitAuras> aurasToRemove)
	{
		if (!playerIdToGameStateData.TryGetValue(desiredPlayerId, out var data)) return;
		if (aurasToRemove.Count == 0) return;

		var newAuraDictionary = new Dictionary<GrubbitEnums.GrubbitAuras, int>();
		newAuraDictionary.AddRange(data.activeAurasList);

		data.activeAurasList.ForEach(aura =>
		{
			if (aurasToRemove.Contains(aura.Key))
			{
				newAuraDictionary.Remove(aura.Key);
			}
		});

		if (data.activeAurasList.Count == newAuraDictionary.Count) return;

		data.activeAurasList.Clear();
		data.activeAurasList.AddRange(newAuraDictionary);

		GrubbitGameManager.Instance.SessionNetworkEvents.RemoveAurasFromPlayerClientRpc(desiredPlayerId, aurasToRemove);
	}

	public void Server_RemoveAuraFromPlayer(string desiredPlayerId, GrubbitEnums.GrubbitAuras aurasToRemove)
	{
		if (!playerIdToGameStateData.TryGetValue(desiredPlayerId, out var data)) return;

		if (data.activeAurasList.ContainsKey(aurasToRemove))
		{
			data.activeAurasList.Remove(aurasToRemove);
		}

		GrubbitGameManager.Instance.SessionNetworkEvents.RemoveAuraFromPlayerClientRpc(desiredPlayerId, aurasToRemove);
	}

	private void ApplyPreCombatAuras(CardBase cardObject, string playerIdOwner)
	{
		if (!playerIdToGameStateData.TryGetValue(playerIdOwner, out var data)) return;
		if (data.activeAurasList == null || data.activeAurasList.Count == 0) return;

		var aurasToRemove = new List<GrubbitEnums.GrubbitAuras>();
		var cardType = GrubbitCardUtility.GetCardDataById(cardObject.CardId).type;
		if (cardType == GrubbitEnums.CardType.ToadCard)
		{
			foreach (var auraKeyValuePair in data.activeAurasList)
			{
				switch (auraKeyValuePair.Key)
				{
					case GrubbitEnums.GrubbitAuras.Akimbo:
						cardObject.CurrentHealth = Mathf.Clamp(cardObject.CurrentHealth - 2, 0, 99);
						cardObject.StatsAdjusted = true;
						aurasToRemove.Add(GrubbitEnums.GrubbitAuras.Akimbo);
						break;
					case GrubbitEnums.GrubbitAuras.AWorthySacrificeDamage:
						cardObject.CurrentHealth = Mathf.Clamp(cardObject.CurrentHealth - 4, 0, 99);
						cardObject.StatsAdjusted = true;
						aurasToRemove.Add(GrubbitEnums.GrubbitAuras.AWorthySacrificeDamage);
						break;
					case GrubbitEnums.GrubbitAuras.AWorthySacrificeDestroy:
						cardObject.CurrentHealth = 0;
						cardObject.StatsAdjusted = true;
						aurasToRemove.Add(GrubbitEnums.GrubbitAuras.AWorthySacrificeDestroy);
						break;
					case GrubbitEnums.GrubbitAuras.BagOfTricks:
						cardObject.CurrentHealth += 1;
						cardObject.CurrentPower += 1;
						cardObject.StatsAdjusted = true;
						aurasToRemove.Add(GrubbitEnums.GrubbitAuras.BagOfTricks);
						break;
				}
			}
		}

		if (cardObject.StatsAdjusted)
		{
			GrubbitGameManager.Instance.SessionNetworkEvents.CardStatsAdjustedClientRpc(cardObject.CardId, JsonUtility.ToJson(cardObject.CardStats));
		}

		if (aurasToRemove.Count > 0)
		{
			Server_RemoveAurasFromPlayer(playerIdOwner, aurasToRemove);
		}
	}
}

