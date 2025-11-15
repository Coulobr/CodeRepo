using Grubbit;
using UnityEngine;

/// <summary>
/// Choose to either
/// Lose 1 Power permanently
/// to deal 4 Damage to your next Toad Card this Match Plot
/// or lose 3 Power permanently to destroy it instead.
/// </summary>
public class CardAWorthySacrifice : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		gameSession.Server_MultipleChoice(gameSession.Server_AWorthySacrifice, CardId, CastersPlayerId, CastersPlayerId);
		return true;
	}
}