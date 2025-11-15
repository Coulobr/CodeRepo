// UIBoardFitter.cs
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIBoardFitter : MonoBehaviour
{
	[Header("Refs")]
	public RectTransform boardRect;      // This is usually the same object this script is on
	public UIGridSystem grid;            // Your UI grid (width/height)

	[Header("Padding (Pixels)")]
	public float topPadPx = 140f;
	public float bottomPadPx = 140f;
	public float leftPadPx = 60f;
	public float rightPadPx = 60f;

	[Header("Additional % Padding (0–1 of safe area size)")]
	[Range(0, 0.2f)] public float topPadPct = 0.0f;
	[Range(0, 0.2f)] public float bottomPadPct = 0.0f;
	[Range(0, 0.2f)] public float leftPadPct = 0.0f;
	[Range(0, 0.2f)] public float rightPadPct = 0.0f;

	[Header("Cell Constraints")]
	public float minCell = 36f;  // clamp too small
	public float maxCell = 120f; // clamp too large

	[Header("Fit")]
	public bool respectSafeArea = true;  // use device safe area
	public bool centerBoard = true;      // keep board centered

	Rect _lastSafeArea;

	void Reset()
	{
		boardRect = GetComponent<RectTransform>();
		grid = FindFirstObjectByType<UIGridSystem>();
	}

	void OnEnable()
	{
		Apply();
	}

	void Update()
	{
#if UNITY_EDITOR
		Apply(); // keep live in editor
#else
        // At runtime, only re-apply if safe area / size changed
        var s = GetSafeAreaRectInCanvasSpace();
        if (s != _lastSafeArea) Apply();
#endif
	}

	void OnRectTransformDimensionsChange()
	{
		Apply();
	}

	void Apply()
	{
		if (!boardRect || !grid || grid.width <= 0 || grid.height <= 0) return;

		// Canvas space rect we can use (safe area aware)
		var usable = GetUsableRect();

		// Apply extra % padding relative to usable size
		float xPadPctL = usable.width * leftPadPct;
		float xPadPctR = usable.width * rightPadPct;
		float yPadPctT = usable.height * topPadPct;
		float yPadPctB = usable.height * bottomPadPct;

		float innerW = Mathf.Max(0, usable.width - leftPadPx - rightPadPx - xPadPctL - xPadPctR);
		float innerH = Mathf.Max(0, usable.height - topPadPx - bottomPadPx - yPadPctT - yPadPctB);

		// Target aspect of the grid
		float gridAspect = (float)grid.width / (float)grid.height;

		// Fit the board inside inner rect while preserving aspect
		float boardW = innerW;
		float boardH = innerW / gridAspect;
		if (boardH > innerH)
		{
			boardH = innerH;
			boardW = innerH * gridAspect;
		}

		// Compute cell size (square), clamped
		float cellFromW = boardW / grid.width;
		float cellFromH = boardH / grid.height;
		float cell = Mathf.Floor(Mathf.Min(cellFromW, cellFromH));
		cell = Mathf.Clamp(cell, minCell, maxCell);

		// Recompute the final board size to be exact multiples of cell size
		boardW = cell * grid.width;
		boardH = cell * grid.height;

		// Size the board rect
		boardRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boardW);
		boardRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, boardH);
		if (centerBoard)
			boardRect.anchoredPosition = Vector2.zero;

		// Push to grid so it knows how to place cells
		grid.cellSize = cell;
		// origin should be bottom-left inside the board, assuming board pivot is center (0.5, 0.5)
		grid.origin = new Vector2(-boardW * 0.5f, -boardH * 0.5f);
		grid.ReapplySizesForAllBlocks();

		_lastSafeArea = GetSafeAreaRectInCanvasSpace();

		// Make sure grid knows which board it’s sizing
		if (!grid.boardRect) grid.boardRect = boardRect;

		// Let any blocks react (even before grid.Init())
		if (boardRect)
			boardRect.BroadcastMessage("OnCellSizeChanged", grid.cellSize, SendMessageOptions.DontRequireReceiver);

		// Only touch grid cells if the grid is actually initialized
		if (grid.Initialized)
			grid.ReapplySizesForAllBlocks();
	}

	Rect GetUsableRect()
	{
		// Convert safe area to the local space of the canvas root
		var safeLocal = respectSafeArea ? GetSafeAreaRectInCanvasSpace() : GetCanvasRect();
		// We want usable rect in the same local space as boardRect’s parent (usually Canvas)
		// If your boardRect is not under the Canvas root, adjust here accordingly.
		return safeLocal;
	}

	Rect GetCanvasRect()
	{
		var canvas = GetComponentInParent<Canvas>();
		if (!canvas || !canvas.pixelRect.size.sqrMagnitude.Equals(canvas.pixelRect.size.sqrMagnitude)) // guard
			return new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height));

		// Convert full screen rect to canvas local space
		Vector2 min, max;
		RectTransform canvasRT = canvas.transform as RectTransform;
		Rect full = new Rect(0, 0, Screen.width, Screen.height);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, full.min, canvas.worldCamera, out min);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, full.max, canvas.worldCamera, out max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}

	Rect GetSafeAreaRectInCanvasSpace()
	{
		var canvas = GetComponentInParent<Canvas>();
		if (!canvas) return GetCanvasRect();

		Rect sa = Screen.safeArea;
		RectTransform canvasRT = canvas.transform as RectTransform;

		Vector2 min, max;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, sa.min, canvas.worldCamera, out min);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, sa.max, canvas.worldCamera, out max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}
}
