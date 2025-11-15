using Grubbit;
using UnityEngine;

/// <summary>
/// Raise the Power and Health of your opponent's next Toad Card event this Match Plot by 1.
/// </summary>
public class CardBagOfTricks : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		gameSession.Server_AddAuraToPlayer(CastersOpponentClientId, GrubbitEnums.GrubbitAuras.BagOfTricks, 1, true);
		return true;
	}
}