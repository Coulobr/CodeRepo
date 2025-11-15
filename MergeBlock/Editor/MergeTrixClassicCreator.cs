#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MergeTrixClassicCreator
{
    private const string RootDataPath = "Assets/Data";
    private const string BlocksPath = RootDataPath + "/Blocks";
    private const string PiecesPath = RootDataPath + "/Pieces";
    private const string PieceSetsPath = RootDataPath + "/PieceSets";

    [MenuItem("MergeTrix/Auto-Create/Classic Set (Blocks 1-5 + 7 Pieces + PieceSet)")]
    public static void CreateClassicSet()
    {
        EnsureFolder(RootDataPath);
        EnsureFolder(BlocksPath);
        EnsureFolder(PiecesPath);
        EnsureFolder(PieceSetsPath);

        // 1) BlockData 1..5
        var blocks = new List<BlockDataSO>();
        for (int v = 1; v <= 5; v++)
        {
            string assetPath = $"{BlocksPath}/Block_{v}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BlockDataSO>(assetPath);
            if (existing == null)
            {
                var bd = ScriptableObject.CreateInstance<BlockDataSO>();
                bd.name = $"Block_{v}";
                bd.value = v;
                bd.color = DefaultColor(v);
                AssetDatabase.CreateAsset(bd, assetPath);
                blocks.Add(bd);
            }
            else
            {
                blocks.Add(existing);
            }
        }

        var block1 = blocks[0];

        // 2) 7 classic pieces
        var pieceAssets = new List<PieceDefinitionSO>();
        pieceAssets.Add(CreatePiece("Piece_I",  new[] { V(0,0), V(1,0), V(2,0), V(3,0) }, block1));
        pieceAssets.Add(CreatePiece("Piece_O",  new[] { V(0,0), V(1,0), V(0,1), V(1,1) }, block1));
        pieceAssets.Add(CreatePiece("Piece_T",  new[] { V(0,0), V(1,0), V(2,0), V(1,1) }, block1));
        pieceAssets.Add(CreatePiece("Piece_L",  new[] { V(0,0), V(0,1), V(0,2), V(1,0) }, block1));
        pieceAssets.Add(CreatePiece("Piece_J",  new[] { V(1,0), V(1,1), V(1,2), V(0,0) }, block1));
        pieceAssets.Add(CreatePiece("Piece_S",  new[] { V(1,0), V(2,0), V(0,1), V(1,1) }, block1));
        pieceAssets.Add(CreatePiece("Piece_Z",  new[] { V(0,0), V(1,0), V(1,1), V(2,1) }, block1));

        // 3) PieceSet asset
        string setPath = $"{PieceSetsPath}/PieceSet_Classic.asset";
        var existingSet = AssetDatabase.LoadAssetAtPath<PieceSetSO>(setPath);
        if (existingSet == null)
        {
            var set = ScriptableObject.CreateInstance<PieceSetSO>();
            set.pieces = pieceAssets.ToArray();
            AssetDatabase.CreateAsset(set, setPath);
            Debug.Log($"Created PieceSet at {setPath} with {set.pieces.Length} pieces.");
        }
        else
        {
            existingSet.pieces = pieceAssets.ToArray();
            EditorUtility.SetDirty(existingSet);
            Debug.Log($"Updated existing PieceSet at {setPath} with {existingSet.pieces.Length} pieces.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("MergeTrix Classic Set", "Created/updated Blocks 1-5, 7 classic Pieces, and PieceSet_Classic under Assets/Data/.", "OK");
    }

    private static PieceDefinitionSO CreatePiece(string name, Vector2Int[] localCells, BlockDataSO defaultBlock)
    {
        string path = $"{PiecesPath}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PieceDefinitionSO>(path);
        if (existing != null)
        {
            existing.localCells = localCells;
            existing.defaultBlockData = defaultBlock;
            existing.pivot = new Vector2(0.5f, 0.5f);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        var piece = ScriptableObject.CreateInstance<PieceDefinitionSO>();
        piece.name = name;
        piece.localCells = localCells;
        piece.defaultBlockData = defaultBlock;
        piece.pivot = new Vector2(0.5f, 0.5f);
        AssetDatabase.CreateAsset(piece, path);
        return piece;
    }

    private static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/');
        string acc = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = acc + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(acc, parts[i]);
            acc = next;
        }
    }

    private static Color DefaultColor(int v)
    {
        switch (v)
        {
            case 1: return new Color(0.2f, 0.6f, 1f);   // blue
            case 2: return new Color(0.2f, 0.8f, 0.4f); // green
            case 3: return new Color(1f, 0.7f, 0.2f);   // orange
            case 4: return new Color(0.9f, 0.3f, 0.3f); // red
            case 5: return new Color(0.8f, 0.4f, 1f);   // purple
            default: return Color.white;
        }
    }
}
#endif