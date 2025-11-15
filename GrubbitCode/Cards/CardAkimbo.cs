using System.Linq;
using Grubbit;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Reward: Deal 1 Damage to your opponent and 2 Damage to your next Toad Card this Match Plot.
/// </summary>
public class CardAkimbo: CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		gameSession.Server_DealDamage(1, CardId, CastersPlayerId, CastersOpponentClientId);
		gameSession.Server_AddAuraToPlayer(CastersPlayerId, GrubbitEnums.GrubbitAuras.Akimbo, 1);
		return true;
	}
}