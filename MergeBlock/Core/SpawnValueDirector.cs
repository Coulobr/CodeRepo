using UnityEngine;

public class SpawnValueDirector : MonoBehaviour
{
	[Header("Adaptive Value Settings")]
	public int minValue = 1;
	public int maxValueCap = 999;          // allow big growth if your game goes long
	public int jitter = 1;                 // ± random wobble (keep small)
	[Range(0.01f, 0.9f)] public float ema = 0.2f;

	// 10% under the smoothed average target
	[Tooltip("Percentage under the smoothed average to set as the spawn target (0.10 = 10% under).")]
	[Range(0f, 0.5f)] public float underPct = 0.10f;

	float _smoothedAvg = 1f;

	public void ResetAvg(int seed = 1) => _smoothedAvg = Mathf.Max(1, seed);

	public void ObserveBoardAverage(float boardAvg)
	{
		if (boardAvg <= 0f) boardAvg = 1f;
		_smoothedAvg = Mathf.Lerp(_smoothedAvg, boardAvg, ema);
	}

	public int NextSpawnValue()
	{
		// target = 90% of smoothed average
		float target = Mathf.Max(1f, _smoothedAvg * (1f - underPct));

		int baseVal = Mathf.Max(minValue, Mathf.Min(maxValueCap, Mathf.RoundToInt(target)));
		int v = baseVal + Random.Range(-jitter, jitter + 1);
		return Mathf.Clamp(v, minValue, maxValueCap);
	}
}