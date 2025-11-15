using System.Linq;
using Grubbit;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Your opponent reveals their next event.
/// If it's an Activity, destroy it. If it's a Toad Card, fight it for them
/// </summary>
public class CardAmbush : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		if (!base.Resolve(casterPlayerId, playerIdRewardRecipient)) return false;

		var opponentData = gameSession.GetPlayerGameStateData(CastersOpponentClientId);
		var opponentRemainingEvents = opponentData.cardsInMatchPlot.Except(opponentData.cardsInPlay).ToList();
		
		if (opponentRemainingEvents.Count <= 0) return false;

		var cardIdToReveal = opponentRemainingEvents[0];
		gameSession.Server_RevealEventCard(cardIdToReveal.CardId, CastersOpponentClientId);
		switch (GrubbitCardUtility.GetCardDataById(cardIdToReveal.CardId).type)
		{
			case GrubbitEnums.CardType.Activity:
				gameSession.Server_DestroyCard(cardIdToReveal, CastersOpponentClientId); 
				break;
			case GrubbitEnums.CardType.ToadCard:
				gameSession.Server_TryResolveCard(cardIdToReveal.CardId, CastersOpponentClientId, CastersPlayerId, CastersPlayerId);
				break;
			default: return false;
		}

		return true;
	}
}