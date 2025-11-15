using UnityEngine;

[RequireComponent(typeof(FormulaBook))]
public class AutoAttackScheduler : MonoBehaviour
{
	FormulaBook book;

	void Awake() { book = GetComponent<FormulaBook>(); }

	void Update()
	{
		float dt = Time.deltaTime;
		foreach (var s in book.learnedSpells)
		{
			if (!s) continue;
			s.Tick(dt);
			if (s.Ready) s.Cast();
		}
	}
}