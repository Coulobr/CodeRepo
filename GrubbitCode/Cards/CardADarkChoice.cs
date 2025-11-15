using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;

namespace Grubbit
{
	/// <summary>
	/// If your last event was a Council card, gain 6 Grub.
	/// If it was a Nomad card, equip a <b>2/2 Weapon</b>.
	/// If it was a Dweller card, drain your opponent for 4 Health.
	/// </summary>
	public class CardADarkChoice : CardBase
	{
		public override bool Resolve(string playerIdCaster, string playerIdRewardRecipient)
		{
			if (!base.Resolve(playerIdCaster, playerIdRewardRecipient)) return false;

			var lastPlayedCardId = gameSession.GetPlayerGameStateData(CastersPlayerId).lastPlayedCardId;
			var lastCardDataSubType = GrubbitCardUtility.GetCardDataById(lastPlayedCardId).subType;

			if (string.IsNullOrWhiteSpace(CardId) || string.IsNullOrWhiteSpace(lastPlayedCardId)) return false;

			switch (lastCardDataSubType)
			{
				case GrubbitEnums.CardSubType.Council:
					gameSession.Server_AddGrub(6, CardId, CastersPlayerId);
					break;
				case GrubbitEnums.CardSubType.Nomad:
					gameSession.Server_EquipWeapon(2, 2, CastersPlayerId);
					break;
				case GrubbitEnums.CardSubType.Dweller:
					gameSession.Server_DrainHealth(4, CardId, CastersPlayerId, CastersOpponentClientId);
					break;
			}

			return true;
		}
	}
}