using Grubbit;
using UnityEngine;

/// <summary>
/// Your opponent gains 5 Armor. 
/// Steal 2 Grub from your opponent.
/// </summary>
public class CardAFriendlyWager : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		var opponentCurrentGrub = gameSession.GetPlayerGameStateData(CastersOpponentClientId).grub;
		var gubToSteal = opponentCurrentGrub >= 2 ? 2 : opponentCurrentGrub;

		if (opponentCurrentGrub == -999) return false;

		gameSession.Server_AddArmor(5, CardId, CastersOpponentClientId);
		gameSession.Server_AddGrub(gubToSteal, CardId, CastersPlayerId);
		gameSession.Server_AddGrub(-gubToSteal, CardId, CastersOpponentClientId);

		return true;
	}
}


