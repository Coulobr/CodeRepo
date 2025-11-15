using System.Collections.Generic;
using System.Linq;
using Grubbit;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Reward: If your hand is empty, destroy the rest of your opponent's Match Plot
/// This Card Is Gone For Good.
/// </summary>
public class CardAlesiaTheAssassin : CardBase
{
	public override bool ToBeExiled => true;

	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;
		if (gameSession.GetPlayerGameStateData(CastersPlayerId).cardsInHand.Count > 0) return false;

		var opponentData = gameSession.GetPlayerGameStateData(CastersOpponentClientId);
		if (opponentData.cardsInMatchPlot.Count <= 0) return false;

		var cardsToDestroy = new List<string>(4);
		cardsToDestroy.AddRange(opponentData.cardsInMatchPlot.Where(card => !opponentData.cardsInPlay.Contains(card)).Select(card => card.CardId));

		gameSession.Server_DestroyCards(cardsToDestroy, CastersOpponentClientId);
		return true;
	}
}