using UnityEngine;

public class UIScoreBinder : MonoBehaviour
{
	public UIScoreCounter counter;

	void Awake()
	{
		if (!counter) counter = GetComponent<UIScoreCounter>();
	}

	void OnEnable()
	{
		GameEvents.OnScoreChanged += HandleScoreChanged;
	}

	void OnDisable()
	{
		GameEvents.OnScoreChanged -= HandleScoreChanged;
	}

	void HandleScoreChanged(int newScore)
	{
		if (counter) counter.AnimateTo(newScore); // smooth tally to the authoritative score
	}
}
