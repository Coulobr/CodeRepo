using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    public TextMeshProUGUI GameOverText { get; private set; }
    private TextMeshProUGUI gameOverText;

    void Awake()
    {
        GameEvents.OnGameOver -= OnGameOver;
        GameEvents.OnGameOver += OnGameOver;

		GameEvents.OnGameStarted -= OnGameStarted;
		GameEvents.OnGameStarted += OnGameStarted;

		gameOverText = GetComponentInChildren<TextMeshProUGUI>();
	}

    private void OnGameOver()
    {
		gameOverText.enabled = true;
	}

	private void OnGameStarted()
	{
		gameOverText.enabled = false;
	}
}
