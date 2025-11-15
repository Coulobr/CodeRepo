using System.Linq;
using Grubbit;
using UnityEngine;

/// <summary>
/// Steal 2 Armor and drain your opponent for 1 Health for each time you've played this Activity
/// </summary>
public class CardAcidBlast : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		var opponentCurrentArmor = gameSession.GetPlayerGameStateData(CastersOpponentClientId).armor;
		var armorToStealToSteal = opponentCurrentArmor >= 2 ? 2 : opponentCurrentArmor;
		var timesPlayed = gameSession.GetPlayerGameStateData(CastersPlayerId).cardsPlayed.FindAll(x => x == CardId).Count;

		gameSession.Server_AddGrub(armorToStealToSteal, CardId, CastersPlayerId);
		gameSession.Server_AddGrub(-armorToStealToSteal, CardId, CastersOpponentClientId);
		gameSession.Server_DrainHealth(Mathf.Clamp(timesPlayed, 1, 99), CardId, CastersPlayerId, CastersOpponentClientId);

		return true;
	}
}
