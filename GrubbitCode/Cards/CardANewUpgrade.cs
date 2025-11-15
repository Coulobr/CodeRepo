using Grubbit;
using UnityEngine;

/// <summary>
/// Gain +2 Power permanently
/// </summary>
public class CardANewUpgrade : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		gameSession.Server_AddHeroPowerPermanent(2, CardId, CastersPlayerId);
		return true;
	}
}