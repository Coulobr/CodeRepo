using UnityEngine;

[CreateAssetMenu(menuName = "MergeTrix/Piece Set", fileName = "PieceSet")]
public class PieceSetSO : ScriptableObject
{
    public PieceDefinitionSO[] pieces;
}