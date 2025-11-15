using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGhostRenderer : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform boardRect;
    public UIGridSystem grid;

    [Header("Style")]
    public Sprite ghostSprite;
    [Range(0f, 1f)] public float alpha = 0.35f;
    public Color color = Color.white;

    private readonly List<Image> _ghostCells = new();

    void Reset()
    {
        grid = FindFirstObjectByType<UIGridSystem>();
        if (grid) boardRect = grid.boardRect;
    }

    public void ShowAt(System.Collections.Generic.List<Vector2Int> gridCells)
    {
        if (!grid || !boardRect) return;
        EnsurePool(gridCells.Count);

        int i = 0;
        float s = grid.GetBlockSize();
        foreach (var p in gridCells)
        {
            var img = _ghostCells[i++];
            img.rectTransform.SetParent(boardRect, false);
            img.rectTransform.sizeDelta = new Vector2(s, s);
            img.rectTransform.anchoredPosition = grid.GridToAnchored(p);
            img.enabled = true;
        }
        for (; i < _ghostCells.Count; i++) _ghostCells[i].enabled = false;
    }

    public void Hide()
    {
        foreach (var img in _ghostCells) img.enabled = false;
    }

    void EnsurePool(int count)
    {
        while (_ghostCells.Count < count)
        {
            var go = new GameObject("GhostCell", typeof(RectTransform), typeof(Image));
            var img = go.GetComponent<Image>();
            img.sprite = ghostSprite;
            img.color = new Color(color.r, color.g, color.b, alpha);
            img.raycastTarget = false;
            _ghostCells.Add(img);
        }
        foreach (var img in _ghostCells)
        {
            img.sprite = ghostSprite;
            img.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}
