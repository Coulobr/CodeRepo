using Grubbit;
using UnityEngine;

/// <summary>
/// Reward: If it took more than 1 hit to kill this Toad Card, gain +1 Power permanently.
/// </summary>
public class CardBargainDealer : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		if (CombatHitsTaken <= 1) return false;

		gameSession.Server_AddHeroPowerPermanent(1, CardId, CastersPlayerId);
		return true;
	}
}