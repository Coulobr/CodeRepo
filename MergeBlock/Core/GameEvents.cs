using System;

public static class GameEvents
{
    public static Action OnGameStarted;
    public static Action OnGameOver;
    public static Action OnResetGame;

    public static Action<int> OnScoreChanged;
    public static Action<int> OnLinesCleared;

    public static Action OnPieceSpawned;
    public static Action OnPieceLanded;

    public static Action<int, int> OnBlocksMerged; // (resultValue, mergedCount)
}