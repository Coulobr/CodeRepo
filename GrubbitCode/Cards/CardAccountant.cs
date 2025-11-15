using Grubbit;
using UnityEngine;

/// <summary>
///Reward: Each time you gain Grub this Match Plot, gain that much Grub plus 1 instead.
/// </summary>
public class CardAccountant : CardBase
{
	public override bool Resolve(string casterPlayerId, string playerIdRewardRecipient)
	{
		return base.Resolve(casterPlayerId, playerIdRewardRecipient);
	}
}