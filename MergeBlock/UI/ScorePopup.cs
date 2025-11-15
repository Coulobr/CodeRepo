using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
	public RectTransform rect;
	public CanvasGroup cg;
	public TextMeshProUGUI text;

	[Header("Tuning")]
	public float popUpLift = 40f;     // small lift before flight
	public float popUpTime = 0.12f;   // initial pop
	public float flyTime = 0.30f;   // flight duration
	public float textPunch = 0.15f;   // extra text punch

	void Awake()
	{
		if (!rect) rect = GetComponent<RectTransform>();
		if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
		if (!text) text = GetComponentInChildren<TextMeshProUGUI>();
	}

	// localStart/Target are in parent (FX layer) local space
	public void Play(int points, Vector2 localStart, Vector2 localTarget, System.Action onArrive = null)
	{
		if (text)
		{
			text.text = points != 0 ? $"+{points}" : text.text; // allow 0 for “banner-like” reuse
			text.transform.localScale = Vector3.one * 0.95f;
		}

		cg.alpha = 0f;
		rect.anchoredPosition = localStart;
		rect.localScale = Vector3.one * 0.9f;

		var seq = DOTween.Sequence();

		// appear + small lift (OutQuad feel)
		seq.Append(cg.DOFade(1f, 0.06f));
		seq.Join(rect.DOScale(1.0f, popUpTime).SetEase(Ease.OutBack));
		seq.Join(rect.DOAnchorPos(localStart + new Vector2(0f, popUpLift), popUpTime).SetEase(Ease.OutQuad));

		// extra text punch for readability
		if (text)
			seq.Join(text.transform.DOPunchScale(Vector3.one * textPunch, popUpTime * 1.2f, 10, 0.8f));

		// fly toward target (ease OutQuad)
		seq.Append(rect.DOAnchorPos(localTarget, flyTime).SetEase(Ease.OutQuad));
		seq.Join(cg.DOFade(0.0f, flyTime));

		seq.OnComplete(() =>
		{
			onArrive?.Invoke();
			Destroy(gameObject);
		});
	}
}
