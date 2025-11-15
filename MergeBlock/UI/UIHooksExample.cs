using UnityEngine;
using UnityEngine.UI;

public class UIHooksExample : MonoBehaviour
{
    public Text scoreText;
    public Text linesText;

    void OnEnable()
    {
        GameEvents.OnScoreChanged += HandleScore;
        GameEvents.OnLinesCleared += HandleLines;
    }
    void OnDisable()
    {
        GameEvents.OnScoreChanged -= HandleScore;
        GameEvents.OnLinesCleared -= HandleLines;
    }

    void HandleScore(int s) { if (scoreText) scoreText.text = $"Score: {s}"; }
    void HandleLines(int l) { if (linesText) linesText.text = $"Lines: {l}"; }
}