using UnityEngine;

[CreateAssetMenu(menuName = "AA/Combo Recipe")]
public class ComboRecipeSO : ScriptableObject
{
	public ReagentSO A;
	public ReagentSO B;
	public SpellSO result;
}