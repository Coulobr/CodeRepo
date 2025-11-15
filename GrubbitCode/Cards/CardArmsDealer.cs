using Grubbit;
using UnityEngine;

/// <summary>
/// Reward: Destroy your Weapon to equip a 2/5 Weapon
/// </summary>
public class CardArmsDealer : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		gameSession.Server_EquipWeapon(2, 5, CastersPlayerId);
		return true;
	}
}