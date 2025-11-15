using UnityEngine;

public class ScoreFXManager : MonoBehaviour
{
	[Header("Refs")]
	public RectTransform fxLayer;       // full-screen layer under Canvas
	public RectTransform boardRect;     // UIGridSystem.boardRect
	public RectTransform scoreTextRect; // TMP score RectTransform
	public ScorePopup popupPrefab;     // for points
	public BannerPopup bannerPrefab;    // for NICE/GREAT/AWESOME/EXCELLENT
	public UIScoreCounter counter;      // (if you still use direct tally here)

	Vector2 BoardAnchoredToFxLocal(Vector2 boardAnchored)
	{
		Vector3 world = boardRect.TransformPoint(boardAnchored);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			fxLayer,
			RectTransformUtility.WorldToScreenPoint(null, world),
			null,
			out var local);
		return local;
	}

	public void AwardPoints(int points, Vector2 boardAnchoredSource)
	{
		if (!popupPrefab || !fxLayer || !scoreTextRect || !boardRect)
			return;

		var popup = Instantiate(popupPrefab, fxLayer);
		var startLocal = BoardAnchoredToFxLocal(boardAnchoredSource);

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			fxLayer,
			RectTransformUtility.WorldToScreenPoint(null, scoreTextRect.position),
			null,
			out var targetLocal);

		popup.Play(points, startLocal, targetLocal, onArrive: null); // UI tally is driven by OnScoreChanged
	}

	public void ShowBannerUpperCenter(string message)
	{
		if (!bannerPrefab || !fxLayer) return;

		var banner = Instantiate(bannerPrefab, fxLayer);

		// Ensure upper-center placement in local space
		var rt = banner.GetComponent<RectTransform>();
		rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.85f); // upper center area
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.anchoredPosition = Vector2.zero; // at that anchor

		banner.Play(message);
	}
}
