using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIBlockRuntime : MonoBehaviour
{
	public RectTransform Rect { get; private set; }
	public Image Image { get; private set; }

	public BlockDataSO Data { get; private set; }
	public int Value { get; private set; }
	public Vector2Int GridPosition { get; set; }
	public bool JustPlaced { get; set; }
	public TextMeshProUGUI BlockMergeGroupText { get; set; }

	// Shared white sprite for tinting when no art is assigned
	static Sprite _whiteSprite;
	static Sprite WhiteTintSprite()
	{
		if (_whiteSprite) return _whiteSprite;
		var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
		tex.Apply(false, true);
		_whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
		_whiteSprite.name = "TintWhiteSprite_Runtime";
		return _whiteSprite;
	}

	void Awake()
	{
		Rect = GetComponent<RectTransform>();
		Image = GetComponent<Image>();
		BlockMergeGroupText = GetComponentInChildren<TextMeshProUGUI>();

		// Make sure Image is in a tintable state
		if (Image)
		{
			Image.material = null;                  // ensure default UI material (tintable)
			Image.type = Image.Type.Simple;
			Image.preserveAspect = true;
			if (Image.sprite == null) Image.sprite = WhiteTintSprite();
		}
	}

	public void Init(BlockDataSO data)
	{
		Data = data;
		Value = data ? Mathf.Max(1, data.value) : 1;

		if (Image)
		{
			// If you haven't provided art yet, we use a tiny white sprite so color tint works
			Image.sprite = (data && data.sprite) ? data.sprite : WhiteTintSprite();
			var c = (data ? data.color : Color.white);
			if (c.a <= 0f) c.a = 1f;               // avoid invisible if alpha accidentally 0
			Image.color = c;
			Image.preserveAspect = true;
			Image.material = null;                 // keep default tintable material
			Image.type = Image.Type.Simple;
		}

		if (BlockMergeGroupText)
			BlockMergeGroupText.text = Value.ToString(); // or hide if you don’t want a label here

		Rect.localScale = Vector3.one * 0.01f;
		Rect.DOScale(1f, 0.2f).SetEase(Ease.OutBack);

		if (!string.IsNullOrEmpty(Data?.spawnSfx))
			SfxSystem.Instance?.Play(Data.spawnSfx, Vector3.zero);

		JustPlaced = true;
	}

	public void SetValue(int newValue)
	{
		Value = newValue;
		if (BlockMergeGroupText) BlockMergeGroupText.text = Value.ToString();
		Rect.DOPunchScale(Vector3.one * 0.2f, 0.15f, 8, 0.6f);
	}

	public void SetValue(int newValue, BlockDataSO newData)
	{
		Value = newValue;

		// Optional visuals swap per value tier
		if (newData != null)
		{
			Data = newData;
			if (Image)
			{
				Image.sprite = newData.sprite ? newData.sprite : WhiteTintSprite();
				var c = newData.color; if (c.a <= 0f) c.a = 1f;
				Image.color = c;
			}
		}
		else
		{
			// No new visuals provided — keep sprite, but ensure color remains from current Data
			if (Image && Data != null)
			{
				var c = Data.color; if (c.a <= 0f) c.a = 1f;
				Image.color = c;
			}
		}

		if (BlockMergeGroupText) BlockMergeGroupText.text = Value.ToString();
		Rect.DOPunchScale(Vector3.one * 0.2f, 0.15f, 8, 0.6f);
	}

	public void OnCellSizeChanged(float newCellSize)
	{
		var parentGrid = GetComponentInParent<UIGridSystem>();
		if (!parentGrid) return;
		float s = Mathf.Max(2f, newCellSize - parentGrid.blockGap);
		Rect.sizeDelta = new Vector2(s, s);
	}

	public void AnimateMerge()
	{
		Rect.DOPunchScale(Vector3.one * 0.3f, 0.2f, 10, 0.8f);
		if (!string.IsNullOrEmpty(Data?.mergeSfx))
			SfxSystem.Instance?.Play(Data.mergeSfx, Vector3.zero);
	}

	public void AnimateFallTo(Vector2 anchored)
	{
		Rect.DOAnchorPos(anchored, 0.08f).SetEase(Ease.Linear);
	}

	public void AnimateMoveTo(Vector2 anchored, float dur)
	{
		Rect.DOAnchorPos(anchored, dur).SetEase(Ease.OutQuad);
	}

	public void AnimateFadeAndDespawn(float dur)
	{
		var cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
		cg.alpha = 1f;
		cg.DOFade(0f, dur).SetEase(Ease.OutQuad).OnComplete(() =>
		{
			cg.alpha = 1f;
			Despawn();
		});
	}

	public void Despawn()
	{
		JustPlaced = false;
		UIBlockPool.Instance.Release(this);
	}
}
