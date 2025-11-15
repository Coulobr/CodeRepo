using UnityEngine;
using Unity.Cinemachine;

public class EnemySpawner : MonoBehaviour
{
	public EnemySimple[] enemyPrefabs;
	public float spawnRadius = 14f;
	public float spawnInterval = 1.2f;
	public AnimationCurve difficultyOverTime = AnimationCurve.Linear(0, 1, 300, 3);

	Transform player;
	float timer;
	float t;

	void Start() { player = GameObject.FindWithTag("Player")?.transform; }

	void Update()
	{
		if (!player || enemyPrefabs.Length == 0) return;
		t += Time.deltaTime;
		timer -= Time.deltaTime;
		if (timer <= 0f)
		{
			timer = spawnInterval / Mathf.Max(0.25f, difficultyOverTime.Evaluate(t));
			SpawnOne();
		}
	}

	void SpawnOne()
	{
		Vector2 pos = (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;
		var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
		Instantiate(prefab, pos, Quaternion.identity);
	}
}
