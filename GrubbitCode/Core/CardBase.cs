using Grubbit;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Grubbit.Server.Cards;
using Grubbit.Server;

[System.Serializable]
public class CardStats
{
	public int baseHealth;
	public int baseArmor;
	public int baseGrubCost;
	public int basePower;
	public int currentHealth;
	public int currentArmor;
	public int currentGrubCost;
	public int currentPower;
}

public class CardBase : MonoBehaviour
{
	public CardStats CardStats { get; set; }

	public int CurrentHealth
	{
		get => CardStats.currentHealth;
		set
		{
			CardStats.currentHealth = Mathf.Clamp(value, 0, 999);
			if (CardStats.currentHealth == 0)
			{

			}
		}
	}

	public int CurrentArmor
	{
		get => CardStats.currentArmor;
		set => CardStats.currentArmor = Mathf.Clamp(value, 0, 999);
	}

	public int CurrentGrubCost
	{
		get => CardStats.currentGrubCost;
		set => CardStats.currentGrubCost = Mathf.Clamp(value, -2, 999);
	}

	public int CurrentPower
	{
		get => CardStats.currentPower;
		set => CardStats.currentPower = Mathf.Clamp(value, 1, 999);
	}

	public int BaseHealth
	{
		get => CardStats.baseHealth;
		set => CardStats.baseHealth = Mathf.Clamp(value, 0, 999);
	}

	public int BaseArmor
	{
		get => CardStats.baseArmor;
		set => CardStats.baseArmor = Mathf.Clamp(value, 0, 999);
	}

	public int BaseGrubCost
	{
		get => CardStats.baseGrubCost;
		set => CardStats.baseGrubCost = Mathf.Clamp(value, -2, 999);
	}

	public int BasePower
	{
		get => CardStats.basePower;
		set => CardStats.basePower = Mathf.Clamp(value, 1, 999);
	}

	public bool StatsAdjusted { get; set; } = false;
	public bool InPlay { get; set; } = false;
	public bool IsExiled { get; set; } = false;
	public bool IsDiscarded { get; set; } = false;
	public string CardId { get; set; }
	public ulong CastersPlayerId { get; set; }
	public ulong CastersOpponentClientId { get; set; }
	public int CombatHitsTaken { get; set; }
	public virtual bool ToBeExiled => false;
	public virtual bool IsBreakEvent => false;

	protected GameSession gameSession = null;

	private void Start()
	{
		InPlay = false;
		IsDiscarded = false;
		IsExiled = false;
		CardStats = new CardStats();
	}

	public void InitializeCard(ulong castersPlayerId, string cardId, int armor, int cost, int health, int power)
	{
		CardId = cardId;
		CastersPlayerId = castersPlayerId;
		CastersOpponentClientId = gameSession.GetOpponentClientId(castersPlayerId);

		BaseArmor = armor;
		BaseGrubCost = cost;
		BaseHealth = health;
		BasePower = power;

		CurrentArmor = armor;
		CurrentHealth = health;
		CurrentPower = power;
		CurrentGrubCost = cost;
	}

	public virtual bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		InPlay = true;

		if (!Utility.IsServer(out var session)) return false;
		gameSession = session;

		return true;
	}

	public virtual bool Resolve(GameSession session, CardContext ctx)
	{
		InPlay = true;

		return true;
	}

	public virtual void OnClearMatchPlot()
	{
		if (ToBeExiled)
		{
			Exile();
		}
		else
		{
			Discard();
		}
	}

	public virtual bool Kill()
	{
		if (!Utility.IsServer(out var server)) return false;
		if (string.IsNullOrWhiteSpace(CardId) || string.IsNullOrWhiteSpace(CastersPlayerId)) return false;

		gameSession = server;
		gameSession.Server_DestroyCard(this, CastersPlayerId);
		return true;
	}

	public virtual bool Discard()
	{
		if (!Utility.IsServer(out var server)) return false;
		if (string.IsNullOrWhiteSpace(CardId) || string.IsNullOrWhiteSpace(CastersPlayerId)) return false;

		gameSession = server;
		InPlay = false;
		IsDiscarded = true;
		IsExiled = false;

		gameSession.Server_DiscardCard(CardId, CastersPlayerId);
		return true;
	}

	public virtual bool Exile()
	{
		if (!Utility.IsServer(out var server)) return false;
		if (string.IsNullOrWhiteSpace(CardId) || string.IsNullOrWhiteSpace(CastersPlayerId)) return false;

		gameSession = server;
		InPlay = false;
		IsDiscarded = false;
		IsExiled = true;

		gameSession.Server_ExileCard(CardId, CastersPlayerId);
		return true;
	}
}
