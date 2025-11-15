using Grubbit;
using UnityEngine;

/// <summary>
/// Gain Grub equal to your Weapon's Power then destroy it.
/// </summary>
public class CardBackfire : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		var data = gameSession.GetPlayerGameStateData(casterPlayerId);
		if (data == null) return false;
		if (data.isWeaponEquipped == false) return false;

		gameSession.Server_AddGrub(data.weaponPower, CardId, CastersPlayerId);
		gameSession.Server_AdjustWeaponStats(0, -data.weaponDurability, CastersPlayerId);
		return true;
	}
}