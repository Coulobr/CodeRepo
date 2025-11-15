using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FormulaBook : MonoBehaviour
{
	[Header("Config")]
	public List<ComboRecipeSO> recipes;

	[Header("Runtime")]
	public List<ReagentSO> knownReagents = new List<ReagentSO>();
	public List<SpellRuntime> learnedSpells = new List<SpellRuntime>();

	public System.Action<SpellSO> onSpellLearned;
	public System.Action<ReagentSO> onReagentAdded;

	// OPTIONAL: seed starting spells in Inspector
	[Header("Starting Spells")]
	public List<SpellSO> startingSpells = new List<SpellSO>();

	void Start()
	{
		// Give the player any starting spells immediately
		foreach (var sp in startingSpells)
		{
			if (sp != null && !HasSpell(sp))
			{
				var sr = sp.AttachTo(gameObject);
				if (sr != null)
				{
					sr.BindDefinition(sp);                         
					sr.Init(transform, sp.baseCooldown);
					learnedSpells.Add(sr);
					onSpellLearned?.Invoke(sp);
				}
			}
		}
	}

	public void AddReagent(ReagentSO r)
	{
		if (!knownReagents.Contains(r))
		{
			knownReagents.Add(r);
			onReagentAdded?.Invoke(r);

			transform.DOPunchScale(Vector3.one * 0.06f, 0.18f, 8, 0.9f);
			TryCraftNewSpells();
		}
	}

	void TryCraftNewSpells()
	{
		for (int i = 0; i < knownReagents.Count; i++)
			for (int j = i + 1; j < knownReagents.Count; j++)
			{
				var rA = knownReagents[i];
				var rB = knownReagents[j];
				var recipe = recipes.Find(rc => (rc.A == rA && rc.B == rB) || (rc.A == rB && rc.B == rA));
				if (recipe != null && recipe.result != null && !HasSpell(recipe.result))  
				{
					var sr = recipe.result.AttachTo(gameObject);
					if (sr != null)
					{
						sr.BindDefinition(recipe.result);                                  
						sr.Init(transform, recipe.result.baseCooldown);
						learnedSpells.Add(sr);
						onSpellLearned?.Invoke(recipe.result);
					}
				}
			}
	}

	// Compare by SO reference (or by runtime.Definition)
	bool HasSpell(SpellSO def)
	{
		foreach (var s in learnedSpells)
			if (s && (s.Definition == def)) return true;
		return false;
	}
}
