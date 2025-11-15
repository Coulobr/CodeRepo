using Grubbit;
using UnityEngine;

/// <summary>
/// Your opponent gains 2 Grub and Discards 1 Card.
/// </summary>
public class CardAuction : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		gameSession.Server_AddGrub(2, CardId, CastersOpponentClientId);
		StartCoroutine(gameSession.Server_PlayerChoiceDiscardCard(1, CastersPlayerId, CastersOpponentClientId));
		return true;
	}
}