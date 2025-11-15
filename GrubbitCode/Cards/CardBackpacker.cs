using Grubbit;
using UnityEngine;

/// <summary>
/// Reward: Draw 1 Card. Gain 3 Grub. Equip a 2/1 Weapon.
/// </summary>
public class CardBackpacker : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		gameSession.Server_DrawCards(1, CastersPlayerId);
		gameSession.Server_AddGrub(3, CardId, CastersPlayerId);
		gameSession.Server_EquipWeapon(2, 1, CastersPlayerId);
		return true;
	}
}