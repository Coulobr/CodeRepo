using Grubbit;
using UnityEngine;

/// <summary>
/// Reward: Draw 2 Cards, then discard a card.
/// Gain 3 Grub.
/// Cards currently in your discard pile are sent to the gone for good pile.
/// </summary>
public class CardAssistant : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		gameSession.Server_DrawCards(2, CastersPlayerId);
		StartCoroutine(gameSession.Server_PlayerChoiceDiscardCard(1, CastersPlayerId, CastersPlayerId, () =>
		{
			gameSession.Server_ExileAllDiscardPile(CastersPlayerId);
		}));
		return true;
	}
}