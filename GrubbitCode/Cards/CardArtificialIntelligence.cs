using Grubbit;
using UnityEngine;

/// <summary>
/// Reward: All of your Weapon's Power is converted to durability.
/// </summary>
public class CardArtificialIntelligence : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		var playerData = gameSession.GetPlayerGameStateData(CastersPlayerId);
		if (playerData == null) return false;

		if (playerData.isWeaponEquipped)
		{
			var weaponDurability = playerData.weaponPower + playerData.weaponDurability;
			gameSession.Server_AdjustWeaponStats(-playerData.weaponPower, weaponDurability, CastersPlayerId);
			return true;
		}

		return false;
	}
}
