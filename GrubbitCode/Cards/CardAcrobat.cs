using System.Linq;
using Grubbit;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Reward: Gain +1 Power permanently.
/// </summary>
public class CardAcrobat : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		gameSession.Server_AddHeroPowerPermanent(1, CardId, CastersPlayerId);
		return true;
	}
}
