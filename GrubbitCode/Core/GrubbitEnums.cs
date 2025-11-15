using System;

public static class GrubbitEnums
{
	public enum ColumnHeaderType
	{
		TextOnly,
		Interactable
	}

	public enum SortOrder
	{
		Ascending,
		Descending
	}

	public enum SortType
	{
		Alphabetical,
		IntValue,
		FloatValue,
		Date
	}

	public enum TooltipType
	{
		Generic = 0
	}

	public enum ToggleItemType
	{
		Image,
		TextMeshProUGUI,
		SmartButton
	}
	public enum MainMenuSidePanelState
	{
		Play,
		Quests,
		Cosmetics
	}

	public enum MatchmakingMode
	{
		Casual,
		Ranked,
	}

	public enum GrubbitGameFormat
	{
		Default,
		Leap,
	}

	public enum CardType
	{
		Activity,
		BreakEvent,
		ToadCard,
		Trinket,
		Hero,
	}

	public enum CardSubType
	{
		Council,
		Dweller,
		Nomad,
		Legendary,
		AllTypes,
	}

	public enum GrubbitAuras
	{
		AnnoyingFly,
		Akimbo,
		AWorthySacrificeDamage,
		AWorthySacrificeDestroy,
		TrinketAcidCoatingSpray,
		TrinketBarrelOfBooze,
		BagOfTricks,
	}

	public static string CardSubtypeToString(CardSubType subType)
	{
		switch (subType)
		{
			case CardSubType.Council: return "Council";
			case CardSubType.Dweller: return "Dweller";
			case CardSubType.Nomad: return "Nomad";
			case CardSubType.Legendary: return "Legendary";
			case CardSubType.AllTypes: return "All Types";
		}

		return "";
	}

	public static string CardTypeToString(CardType type)
	{
		switch (type)
		{
			case CardType.Activity: return "Activity";
			case CardType.BreakEvent: return "Break Event";
			case CardType.ToadCard: return "Toad Card";
			case CardType.Trinket: return "Trinket";
			case CardType.Hero: return "Hero";
		}

		return "";
	}
}


