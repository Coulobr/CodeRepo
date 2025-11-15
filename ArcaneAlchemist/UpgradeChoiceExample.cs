using UnityEngine;
using DG.Tweening;

public class UpgradeChoiceExample : MonoBehaviour
{
	public ReagentSO[] reagentPool;
	FormulaBook book;

	void Awake()
	{
		book = GetComponent<FormulaBook>();
		GetComponent<PlayerLevel>().onLevelUp.AddListener(OnLevelUp);
	}

	void OnLevelUp(int lvl)
	{
		if (reagentPool.Length == 0) return;
		var r = reagentPool[Random.Range(0, reagentPool.Length)];
		book.AddReagent(r);
		Debug.Log($"[Arcane Alchemist] Granted reagent: {r.reagentName}");

		// subtle UI/gameplay feedback
		transform.DOPunchScale(Vector3.one * 0.08f, 0.2f, 8, 0.8f);
	}
}