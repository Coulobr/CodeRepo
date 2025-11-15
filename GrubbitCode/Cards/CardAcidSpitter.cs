using System.Linq;
using Grubbit;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Reward: Remove all of your opponent's Armor. This Card Is Gone For Good.
/// </summary>
public class CardAcidSpitter : CardBase
{
	public override bool ToBeExiled => true;

	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		var opponentsArmor = gameSession.GetPlayerGameStateData(CastersOpponentClientId).armor;
		if (opponentsArmor > 0)
		{
			gameSession.Server_AddArmor(-opponentsArmor, CardId, CastersOpponentClientId);
		}

		return true;
	}
}
