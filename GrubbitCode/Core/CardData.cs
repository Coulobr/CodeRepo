using Grubbit;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CardData", menuName = "GrubbitCardTemplate")]
public class CardData : ScriptableObject
{
	[ShowInInspector]
	[PreviewField(75)]
	 
	public Sprite img;

	public string cardName;
	public GrubbitEnums.CardType type;
	public GrubbitEnums.CardSubType subType;

	public int power;
	public int health;
	public int cost;
	public int armor;
	public string cardId;

	[TextArea(8, 10)]
	public string description;

	[TextArea(8,10)]
	public string flavorText;
}
