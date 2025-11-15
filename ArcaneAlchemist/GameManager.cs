using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public Projectile[] projectiles;
	public int warmCount = 32;
	public UITransitionController uiTransitionController;

	void Awake()
	{
		foreach (var p in projectiles)
			Pool.Preload(p, warmCount);
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {	
		uiTransitionController.LoadSceneWithDissolve("Game Scene");
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
