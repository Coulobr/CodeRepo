using Grubbit;
using UnityEngine;

/// <summary>
/// When you kill a Toad Card, drain your opponent for 2 Health and they Discard 1 Card.
/// </summary>
public class CardBarrelOfBooze : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		gameSession.Server_EquipTrinket(CardId, 3, CastersPlayerId, GrubbitEnums.GrubbitAuras.TrinketBarrelOfBooze);
		return true;
	}
}