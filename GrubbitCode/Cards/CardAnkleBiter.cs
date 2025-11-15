using System.Linq;
using Grubbit;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Reward: Reduce your opponent's Power by 1 permanently.
/// This Card Is Gone For Good.
/// </summary>
public class CardAnkleBiter : CardBase
{
	public override bool ToBeExiled => true;

	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		gameSession.Server_AddHeroPowerPermanent(-1, CardId, CastersOpponentClientId);
		return true;
	}
}