using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PlayerLevel : MonoBehaviour
{
	public UnityEvent<int> onLevelUp;
	public UnityEvent<int, int> onXPChanged; // current, needed

	int level = 1;
	int xp = 0;
	int needed = 5;

	public void AddXP(int amount)
	{
		xp += amount;
		while (xp >= needed)
		{
			xp -= needed;
			level++;
			needed = Mathf.RoundToInt(needed * 1.25f + 2);
			onLevelUp?.Invoke(level);

			// quick celebratory pulse
			transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 8, 0.8f);
		}
		onXPChanged?.Invoke(xp, needed);
	}

	public int Level => level;
}