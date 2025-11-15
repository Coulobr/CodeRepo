using Unity.Collections;
using UnityEngine;

namespace Grubbit
{
	/// <summary>
	/// Equip a  <b>5/2 Weapon</b>.
	/// "It's above average."
	/// </summary>
	public class CardABigSword : CardBase
	{
		public override bool Resolve(string playerIdToEquip, string playerIdRewardRecipient)
		{
			if (!base.Resolve(playerIdToEquip, playerIdRewardRecipient)) return false;
			gameSession.Server_EquipWeapon(5, 2, CastersPlayerId);
			return true;
		}
	}
}
