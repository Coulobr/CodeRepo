using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Place this in a UI Canvas (Screen Space - Overlay). Provide:
/// - canvasGroupBlack: a full-screen CanvasGroup for standard fade.
/// - dissolveTarget: a UIDissolveTarget referencing a full-screen Image that uses a dissolve shader.
/// </summary>
public class UITransitionController : MonoBehaviour
{
	[Header("Fade Overlay")]
	public CanvasGroup canvasGroupBlack;     // full-screen, black Image under a CanvasGroup

	[Header("Dissolve Overlay")]
	public UIDissolveTarget dissolveTarget;  // full-screen Image with dissolve-capable material

	[Header("Defaults")]
	public float defaultFadeTime = 0.5f;
	public float defaultDissolveTime = 0.8f;

	void Awake()
	{
		if (canvasGroupBlack) { canvasGroupBlack.alpha = 0f; canvasGroupBlack.blocksRaycasts = false; }
		if (dissolveTarget) { dissolveTarget.SetDissolve(0f); dissolveTarget.gameObject.SetActive(false); }
	}

	// --------- FADE ----------
	public Tween FadeToBlack(float time = -1f)
	{
		if (!canvasGroupBlack) return null;
		time = time < 0 ? defaultFadeTime : time;
		canvasGroupBlack.blocksRaycasts = true;
		return canvasGroupBlack.DOFade(1f, time);
	}

	public Tween FadeFromBlack(float time = -1f)
	{
		if (!canvasGroupBlack) return null;
		time = time < 0 ? defaultFadeTime : time;
		return canvasGroupBlack.DOFade(0f, time)
			.OnComplete(() => canvasGroupBlack.blocksRaycasts = false);
	}

	// --------- DISSOLVE ----------
	public Tween DissolveTo(float time = -1f)
	{
		if (!dissolveTarget) return null;
		time = time < 0 ? defaultDissolveTime : time;
		dissolveTarget.gameObject.SetActive(true);
		dissolveTarget.SetDissolve(0f);
		return DOTween.To(dissolveTarget.Get, dissolveTarget.SetDissolve, 1f, time);
	}

	public Tween DissolveFrom(float time = -1f)
	{
		if (!dissolveTarget) return null;
		time = time < 0 ? defaultDissolveTime : time;
		dissolveTarget.gameObject.SetActive(true);
		dissolveTarget.SetDissolve(1f);
		return DOTween.To(dissolveTarget.Get, dissolveTarget.SetDissolve, 0f, time)
					  .OnComplete(() => dissolveTarget.gameObject.SetActive(false));
	}

	// --------- Scene helpers ----------
	public void LoadSceneWithFade(string sceneName, float fadeTime = -1f)
	{
		fadeTime = fadeTime < 0 ? defaultFadeTime : fadeTime;
		FadeToBlack(fadeTime).OnComplete(() =>
		{
			SceneManager.LoadScene(sceneName);
			FadeFromBlack(fadeTime * 0.8f);
		});
	}

	public void LoadSceneWithDissolve(string sceneName, float dissolveTime = -1f)
	{
		dissolveTime = dissolveTime < 0 ? defaultDissolveTime : dissolveTime;
		DissolveTo(dissolveTime).OnComplete(() =>
		{
			SceneManager.LoadScene(sceneName);
			DissolveFrom(dissolveTime * 0.8f);
		});
	}
}
