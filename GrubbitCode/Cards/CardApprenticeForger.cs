using Grubbit;
using UnityEngine;

/// <summary>
/// Reward: If you have a Weapon equipped, give it +1/+2. Otherwise, equip a 1/1 Weapon.
/// </summary>
public class CardApprenticeForger : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		var playerData = gameSession.GetPlayerGameStateData(CastersPlayerId);
		if (playerData == null) return false;

		if (playerData.isWeaponEquipped)
		{
			gameSession.Server_AdjustWeaponStats(1, 2, casterPlayerId);
		}
		else
		{
			gameSession.Server_EquipWeapon(1, 1, CastersPlayerId);
		}

		return true;
	}
}