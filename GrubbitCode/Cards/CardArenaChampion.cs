using Grubbit;
using UnityEngine;

/// <summary>
/// Reward: Gain 1 Grub for every stat you have higher than your opponent.
/// This Card Is Gone For Good.
/// </summary>
public class CardArenaChampion : CardBase
{
	public override bool ToBeExiled => true;

	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		var playerData = gameSession.GetPlayerGameStateData(CastersPlayerId);
		var opponentData = gameSession.GetPlayerGameStateData(CastersOpponentClientId);

		int grubToGain = 0;
		if (playerData.armor > opponentData.armor) { grubToGain++; }
		if (playerData.health > opponentData.health) { grubToGain++; }
		if (playerData.heroPower > opponentData.heroPower) { grubToGain++; }
		if (playerData.grub > opponentData.grub) { grubToGain++; }

		gameSession.Server_AddGrub(grubToGain, CardId, CastersPlayerId);
		return true;
	}
}