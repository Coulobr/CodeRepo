
using UnityEngine;
using System.Collections.Generic;

public class UIGridSystem : MonoBehaviour
{
    [Header("Grid")]
    public int width = 10;
    public int height = 20;
    public float cellSize = 48f; // pixels
    public Vector2 origin = Vector2.zero; // bottom-left in board local space

    [Header("Block Fit")]
    [Tooltip("Total gap between adjacent blocks (pixels). Each block shrinks by half of this per side.")]
    public float blockGap = 0f; // set to 0 for no seams

    [Header("Refs")]
    public RectTransform boardRect; // parent for UI blocks

    private UIBlockRuntime[,] _cells;
    public bool Initialized => _cells != null;

    public void Init()
    {
        if (!boardRect) boardRect = GetComponent<RectTransform>();
        if (_cells == null) _cells = new UIBlockRuntime[width, height];
    }

    // SIZE HELPERS ---------------------------------------------------
    public float GetBlockSize() => Mathf.Max(2f, cellSize - blockGap);

    public void ApplyBlockMetrics(UIBlockRuntime b)
    {
        if (!b) return;
        float s = GetBlockSize();
        b.Rect.sizeDelta = new Vector2(s, s);
    }

    public void ReapplySizesForAllBlocks()
    {
        // resize grid-attached blocks if allocated
        if (_cells != null)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (_cells[x, y]) ApplyBlockMetrics(_cells[x, y]);
        }

        // and any free/falling blocks under the board
        if (boardRect)
        {
            foreach (var b in boardRect.GetComponentsInChildren<UIBlockRuntime>(true))
                ApplyBlockMetrics(b);
        }
    }
    // ----------------------------------------------------------------

    public bool InBounds(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
    public bool IsEmpty(Vector2Int p) => InBounds(p) && _cells[p.x, p.y] == null;

    public Vector2 GridToAnchored(Vector2Int gp)
    {
        return new Vector2(origin.x + (gp.x + 0.5f) * cellSize,
                           origin.y + (gp.y + 0.5f) * cellSize);
    }

    public void Place(UIBlockRuntime b, Vector2Int p)
    {
        if (!InBounds(p)) return;
        _cells[p.x, p.y] = b;
        b.GridPosition = p;
        ApplyBlockMetrics(b);
        b.Rect.anchoredPosition = GridToAnchored(p);
    }

    public void Remove(Vector2Int p)
    {
        if (!InBounds(p)) return;
        _cells[p.x, p.y] = null;
    }

    public UIBlockRuntime Get(Vector2Int p) => InBounds(p) ? _cells[p.x, p.y] : null;

    // ==== FIXED: proper enumerator over every cell ====
    public IEnumerable<Vector2Int> AllPositions()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                yield return new Vector2Int(x, y);
    }

    // ==== Row helpers (used by line clears) ====
    public List<int> GetFullRows()
    {
        var rows = new List<int>();
        for (int y = 0; y < height; y++)
        {
            bool full = true;
            for (int x = 0; x < width; x++)
            {
                if (_cells[x, y] == null) { full = false; break; }
            }
            if (full) rows.Add(y);
        }
        return rows;
    }

    public void CollapseRows(List<int> rows)
    {
        if (rows == null || rows.Count == 0) return;
        rows.Sort();

        for (int i = 0; i < rows.Count; i++)
        {
            int row = rows[i];

            // clear row
            for (int x = 0; x < width; x++)
            {
                var b = _cells[x, row];
                if (b != null)
                {
                    _cells[x, row] = null;
                    b.Despawn();
                }
            }

            // shift everything above down by 1
            for (int y = row + 1; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var blk = _cells[x, y];
                    if (blk != null)
                    {
                        _cells[x, y - 1] = blk;
                        _cells[x, y] = null;
                        blk.GridPosition = new Vector2Int(x, y - 1);
                        blk.AnimateFallTo(GridToAnchored(blk.GridPosition));
                    }
                }
            }
        }
    }
}
