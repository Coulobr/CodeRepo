using Grubbit;
using UnityEngine;

/// <summary>
/// Your Armor cannot be removed, stolen or destroyed.
/// </summary>
public class CardAntiAcidCoatingSpray : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		gameSession.Server_EquipTrinket(CardId, 1, CastersPlayerId, GrubbitEnums.GrubbitAuras.TrinketAcidCoatingSpray);
		return true;
	}
}