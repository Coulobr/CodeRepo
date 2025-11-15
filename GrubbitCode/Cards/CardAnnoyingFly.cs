using System.Linq;
using Grubbit;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Your opponent loses 1 Power for their next 3 attacks.
/// </summary>
public class CardAnnoyingFly : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		gameSession.Server_AddHeroPowerPermanent(-1, CardId, CastersOpponentClientId);
		gameSession.Server_AddAuraToPlayer(CastersOpponentClientId, GrubbitEnums.GrubbitAuras.AnnoyingFly, 3);
		return true;
	}
}
