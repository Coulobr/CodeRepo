using System.Collections.Generic;
using UnityEngine;

// ActivePiece now supports per-cell BlockDataSO (so cells in the same piece can differ)
public class ActivePiece
{
    public PieceDefinitionSO def;
    public Vector2Int anchor; // bottom-left in grid

    // Per-cell data aligned to def.localCells order
    public List<BlockDataSO> cellData = new List<BlockDataSO>();

    public List<Vector2Int> CellsWorld(Vector2Int anchorOverride, int rotation)
    {
        var list = new List<Vector2Int>(def.localCells.Length);
        foreach (var c in def.localCells)
        {
            var r = RotateAroundPivot(c, def.pivot, rotation);
            list.Add(anchorOverride + r);
        }
        return list;
    }

    private static Vector2Int RotateAroundPivot(in Vector2Int c, in Vector2 pivot, int rotation)
    {
        float dx = c.x - pivot.x;
        float dy = c.y - pivot.y;
        float rx, ry;
        switch (rotation & 3)
        {
            case 1: rx = -dy; ry = dx; break;
            case 2: rx = -dx; ry = -dy; break;
            case 3: rx = dy;  ry = -dx; break;
            default: rx = dx; ry = dy; break;
        }
        int gx = Mathf.RoundToInt(rx + pivot.x);
        int gy = Mathf.RoundToInt(ry + pivot.y);
        return new Vector2Int(gx, gy);
    }
}