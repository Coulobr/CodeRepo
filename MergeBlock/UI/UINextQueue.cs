using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Next-queue renderer (top-down):
/// - Rows are stacked from the TOP with adjustable top padding and spacing.
/// - Vertical step uses GLOBAL tallest piece height so spacing is consistent.
/// - Each row is centered vertically within its lane; horizontal alignment is selectable.
/// - Optional fixed lane width so you can align to a visual frame (e.g., the white boxes).
/// - Renders per-cell color and value using data from GameManagerUI.PreviewPiece.
/// </summary>
public class UINextQueue : MonoBehaviour
{
	public enum HorizontalAlign { Left, Center, Right }

	[Header("Layout (top-down)")]
	[Tooltip("If null, this component's RectTransform is used.")]
	public RectTransform container;
	[Tooltip("Pixel size of each mini cell.")]
	public float cellSize = 24f;
	[Tooltip("Extra pixels between rows (lane to lane).")]
	public float rowSpacing = 10f;
	[Tooltip("Top padding in pixels before the first row is placed.")]
	public float topPadding = 8f;
	[Tooltip("How many upcoming pieces to show (the queue may pass fewer).")]
	public int previewCount = 5;

	[Header("Horizontal")]
	[Tooltip("Horizontal alignment of each row within the container width.")]
	public HorizontalAlign horizontalAlign = HorizontalAlign.Right;
	[Tooltip("If true, use a fixed lane width instead of hugging the shape width.")]
	public bool useFixedLaneWidth = true;
	[Tooltip("Fixed lane width, in CELL COUNT (multiplied by cellSize). Example: 4 cells * 24px = 96px.")]
	public int fixedLaneCellsX = 4;

	[Header("Appearance")]
	[Tooltip("Fallback sprite if a BlockData has no sprite assigned.")]
	public Sprite fallbackSprite;
	[Tooltip("Text color for the value number overlay.")]
	public Color numberColor = Color.black;
	[Tooltip("Font size for the value overlay.")]
	public int numberFontSize = 18;

	// Pooled UI for each “row”
	private class MiniCell { public Image img; public TextMeshProUGUI txt; }
	private readonly List<List<MiniCell>> _rows = new();      // cells per row
	private readonly List<RectTransform> _rowParents = new(); // one parent per row

	// Global tallest piece height in cells (computed at Awake from the active piece set)
	private int _globalMaxSpanY = 4; // sensible default
	static Sprite _whiteSprite;

	void Awake()
	{
		if (!container) container = GetComponent<RectTransform>();
		if (!container) container = (transform as RectTransform);

		// Compute tallest piece (in cells) to get a consistent lane height
		var gm = FindFirstObjectByType<GameManagerUI>();
		if (gm && gm.pieceSet && gm.pieceSet.pieces != null && gm.pieceSet.pieces.Length > 0)
		{
			int maxY = 1;
			foreach (var def in gm.pieceSet.pieces)
			{
				if (def == null || def.localCells == null || def.localCells.Length == 0) continue;
				int minY = int.MaxValue, maxSpanY = int.MinValue;
				foreach (var c in def.localCells)
				{
					if (c.y < minY) minY = c.y;
					if (c.y > maxSpanY) maxSpanY = c.y;
				}
				int spanY = (maxSpanY - minY + 1);
				if (spanY > maxY) maxY = spanY;
			}
			_globalMaxSpanY = Mathf.Max(1, maxY);
		}
	}

	public void Clear()
	{
		for (int r = 0; r < _rows.Count; r++)
		{
			foreach (var c in _rows[r])
			{
				if (c != null)
				{
					if (c.img) c.img.enabled = false;
					if (c.txt) c.txt.enabled = false;
				}
			}
			if (_rowParents.Count > r && _rowParents[r]) _rowParents[r].gameObject.SetActive(false);
		}
	}
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

	/// <summary>
	/// Pass the actual preview entries (def + per-cell data) from GameManagerUI.
	/// </summary>
	public void SetQueuePreview(IReadOnlyList<GameManagerUI.PreviewPiece> preview)
	{
		if (!container || preview == null) return;

		int n = Mathf.Clamp(previewCount, 0, preview.Count);
		EnsureRowParents(n);

		float laneHeight = _globalMaxSpanY * cellSize; // constant lane height for all rows
		float step = laneHeight + rowSpacing;

		// Stack TOP -> DOWN with top padding
		float y = -topPadding;
		for (int r = 0; r < n; r++)
		{
			var rp = _rowParents[r];
			rp.gameObject.SetActive(true);

			var laneSize = RenderRow(r, rp, preview[r], laneHeight);
			// Position row parent (top anchored)
			rp.anchoredPosition = new Vector2(ComputeRowX(rp, laneSize.x), y);

			// advance to next row slot downward
			y -= step;
		}

		// Hide extra rows if any were pooled previously
		for (int r = n; r < _rowParents.Count; r++)
			_rowParents[r].gameObject.SetActive(false);
	}

	// Ensure one parent RectTransform per row to keep hierarchy tidy
	void EnsureRowParents(int rowsNeeded)
	{
		while (_rowParents.Count < rowsNeeded)
		{
			var rowGO = new GameObject($"NextRow_{_rowParents.Count}", typeof(RectTransform));
			var rt = rowGO.GetComponent<RectTransform>();
			rt.SetParent(container, false);

			// Anchor to top so Y decreases as we go down
			rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); // top-center by default
			rt.pivot = new Vector2(0.5f, 1f);

			_rowParents.Add(rt);
			_rows.Add(new List<MiniCell>());
		}
	}

	/// <summary>
	/// Compute row X based on selected horizontal alignment and row width.
	/// We use the container's width (sizeDelta.x) and its pivot as reference.
	/// </summary>
	float ComputeRowX(RectTransform rowParent, float rowWidth)
	{
		float containerWidth = container.rect.width; // works with driven layout or fixed size
		switch (horizontalAlign)
		{
			case HorizontalAlign.Left:
				// left edge inside container
				return -containerWidth * 0.5f + rowWidth * 0.5f;
			case HorizontalAlign.Right:
				// right edge inside container
				return containerWidth * 0.5f - rowWidth * 0.5f;
			default:
				// center
				return 0f;
		}
	}

	/// <summary>
	/// Renders a single row: centers the shape vertically within its lane and
	/// optionally uses a fixed lane width for horizontal fit/alignment.
	/// Returns the row parent size (width, laneHeight).
	/// </summary>
	Vector2 RenderRow(int rowIndex, RectTransform rowParent, GameManagerUI.PreviewPiece pp, float laneHeight)
	{
		var def = pp.def;
		var cellDatas = pp.cellData;

		// Compute piece bounds (in its own local cell coords)
		int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
		foreach (var c in def.localCells)
		{
			if (c.x < minX) minX = c.x; if (c.x > maxX) maxX = c.x;
			if (c.y < minY) minY = c.y; if (c.y > maxY) maxY = c.y;
		}
		int spanX = (maxX - minX + 1);
		int spanY = (maxY - minY + 1);

		float shapeW = spanX * cellSize;
		float shapeH = spanY * cellSize;

		// Lane width: fixed (recommended to match your white frames) or hug shape
		float laneW = useFixedLaneWidth ? Mathf.Max(cellSize, fixedLaneCellsX * cellSize) : shapeW;

		// Center the shape inside the lane (both axes)
		float originX = -laneW * 0.5f + (laneW - shapeW) * 0.5f; // left edge where shape starts
		float originY = (laneHeight - shapeH) * 0.5f;            // bottom offset within the lane

		// Pool cells for this row
		var list = _rows[rowIndex];
		int needed = def.localCells.Length;
		while (list.Count < needed)
		{
			// mini cell container
			var cellGO = new GameObject($"Cell_{rowIndex}_{list.Count}", typeof(RectTransform));
			var cellRT = cellGO.GetComponent<RectTransform>();
			cellRT.SetParent(rowParent, false);

			// background Image
			var imgGO = new GameObject("Img", typeof(RectTransform), typeof(Image));
			var imgRT = imgGO.GetComponent<RectTransform>();
			imgRT.SetParent(cellRT, false);
			var img = imgGO.GetComponent<Image>();
			img.raycastTarget = false;

			// number Text (TMP)
			var txtGO = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
			var txtRT = txtGO.GetComponent<RectTransform>();
			txtRT.SetParent(cellRT, false);
			var txt = txtGO.GetComponent<TextMeshProUGUI>();
			txt.alignment = TextAlignmentOptions.Center;
			txt.enableAutoSizing = false;
			txt.fontSize = numberFontSize;
			txt.color = numberColor;
			txt.font = TMP_Settings.defaultFontAsset; // LiberationSans SDF default

			list.Add(new MiniCell { img = img, txt = txt });
		}

		// Position each mini cell
		for (int i = 0; i < needed; i++)
		{
			var local = def.localCells[i];
			var data = (i < cellDatas.Count) ? cellDatas[i] : null;
			var mc = list[i];

			float x = originX + (local.x - minX) * cellSize + cellSize * 0.5f;
			float y = originY + (local.y - minY) * cellSize + cellSize * 0.5f;

			var cellRT = mc.img.transform.parent as RectTransform;
			cellRT.sizeDelta = new Vector2(cellSize, cellSize);
			cellRT.anchoredPosition = new Vector2(x, y);

			// image
			mc.img.rectTransform.sizeDelta = cellRT.sizeDelta;
			mc.img.rectTransform.anchoredPosition = Vector2.zero;
			mc.img.sprite = (data && data.sprite) ? data.sprite : (fallbackSprite ? fallbackSprite : WhiteTintSprite());
			mc.img.color = data ? (data.color.a > 0f ? data.color : new Color(data.color.r, data.color.g, data.color.b, 1f)) : Color.white;
			mc.img.material = null; // ensure tintable
			mc.img.type = Image.Type.Simple;
			mc.img.enabled = true;


			// number
			mc.txt.rectTransform.sizeDelta = cellRT.sizeDelta;
			mc.txt.rectTransform.anchoredPosition = Vector2.zero;
			mc.txt.text = data ? data.value.ToString() : "";
			mc.txt.enabled = true;
		}

		// Hide any extra pooled cells not needed by this shape
		for (int i = needed; i < list.Count; i++)
		{
			if (list[i].img) list[i].img.enabled = false;
			if (list[i].txt) list[i].txt.enabled = false;
		}

		// Row parent size: width (fixed or hug) x laneHeight (constant)
		rowParent.sizeDelta = new Vector2(laneW, laneHeight);
		return rowParent.sizeDelta;
	}
}
