using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIScoreCounter : MonoBehaviour
{
	[Header("Refs")]
	public TextMeshProUGUI scoreText;

	[Header("Tuning")]
	public float tallyDuration = 0.35f;
	public System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

	int _displayed;
	int _target;

	void Awake()
	{
		if (!scoreText) scoreText = GetComponentInChildren<TextMeshProUGUI>();
		SetImmediate(0);
	}

	public void SetImmediate(int value)
	{
		_target = _displayed = Mathf.Max(0, value);
		UpdateText();
	}

	public void AnimateTo(int newValue, float? duration = null)
	{
		_target = Mathf.Max(0, newValue);
		DOTween.Kill(this); // kill any prior tween on this component
		DOTween.To(() => _displayed, v => { _displayed = v; UpdateText(); }, _target, duration ?? tallyDuration)
			.SetEase(Ease.OutCubic)
			.SetTarget(this);
	}

	public void Add(int delta, float? duration = null)
	{
		AnimateTo(_target + delta, duration);
	}

	void UpdateText()
	{
		if (scoreText) scoreText.text = _displayed.ToString("#,0", culture);
	}
}