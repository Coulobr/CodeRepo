using UnityEngine;

/// <summary>
/// Deal Damage to your opponent equal to half their Health rounded up.
/// </summary>

namespace Grubbit
{
	public class CardAHardSwing : CardBase
	{
		public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
		{
			if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

			var opponentData = gameSession.GetPlayerGameStateData(CastersOpponentClientId);
			if (opponentData == null) return false;

			var damage = Mathf.RoundToInt(opponentData.health / 2f);
			gameSession.Server_DealDamage(damage, CardId, CastersPlayerId, CastersOpponentClientId);
			return true;
		}
	}
} 
