using UnityEngine;

[CreateAssetMenu(menuName = "MergeTrix/Piece Definition", fileName = "PieceDef")]
public class PieceDefinitionSO : ScriptableObject
{
    public Vector2Int[] localCells;
    public BlockDataSO defaultBlockData;
    public BlockDataSO[] variants;
    public Vector2 pivot = new Vector2(0.5f, 0.5f);
}