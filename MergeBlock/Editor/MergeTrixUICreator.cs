#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MergeTrixUICreator
{
    [MenuItem("MergeTrix/Auto-Create/UI Board + Managers")]
    public static void CreateUIBoardAndManagers()
    {
        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        // Board container
        var boardGO = new GameObject("Board", typeof(RectTransform));
        boardGO.transform.SetParent(canvasGO.transform, false);
        var board = boardGO.GetComponent<RectTransform>();
        board.anchorMin = new Vector2(0.5f, 0.5f);
        board.anchorMax = new Vector2(0.5f, 0.5f);
        board.pivot = new Vector2(0.5f, 0.5f);
        board.sizeDelta = new Vector2(600, 1200); // 10x20 cells @60px, tweak as needed
        board.anchoredPosition = Vector2.zero;

        // Managers root
        var managers = new GameObject("Managers");

        // Grid system (UI)
        var gridGO = new GameObject("UIGridSystem", typeof(RectTransform));
        gridGO.transform.SetParent(managers.transform, false);
        var grid = gridGO.AddComponent<UIGridSystem>();
        grid.boardRect = board;
        grid.width = 10; grid.height = 20; grid.cellSize = 60f;
        grid.origin = new Vector2(-grid.width * grid.cellSize / 2f, -grid.height * grid.cellSize / 2f);

        // Block Pool
        var poolGO = new GameObject("UIBlockPool", typeof(RectTransform));
        poolGO.transform.SetParent(managers.transform, false);
        var pool = poolGO.AddComponent<UIBlockPool>();
        poolGO.GetComponent<RectTransform>().SetParent(board, false); // parent blocks under board by default
        poolGO.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // SFX
        var sfxGO = new GameObject("SfxSystem");
        sfxGO.transform.SetParent(managers.transform, false);
        sfxGO.AddComponent<SfxSystem>();

        // GameManagerUI
        var gmGO = new GameObject("GameManagerUI");
        gmGO.transform.SetParent(managers.transform, false);
        var gm = gmGO.AddComponent<GameManagerUI>();
        gm.grid = grid;
        gm.blockPool = pool;

        // Block prefab (UI Image + UIBlockRuntime)
        var prefabGO = new GameObject("UIBlockPrefab", typeof(RectTransform), typeof(Image));
        var uiBlock = prefabGO.AddComponent<UIBlockRuntime>();
        var img = prefabGO.GetComponent<Image>();
        img.color = Color.gray;
        var rect = prefabGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(grid.cellSize - 4f, grid.cellSize - 4f);
        // Save prefab to Assets
        string dir = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        string path = dir + "/UIBlockPrefab.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(prefabGO, path);
        Object.DestroyImmediate(prefabGO);
        pool.GetType().GetField("prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(pool, prefab.GetComponent<UIBlockRuntime>());
        pool.GetType().GetField("parentForBlocks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(pool, board);

        Selection.activeObject = canvasGO;
        EditorUtility.DisplayDialog("MergeTrix UI", "Created Canvas, Board, Managers, UIBlockPool, SfxSystem, GameManagerUI, and saved UIBlockPrefab.\nAssign your PieceSetSO to GameManagerUI, then press Play.", "OK");
    }
}
#endif