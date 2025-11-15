using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "MergeTrix/Block Data")]
public class BlockDataSO : ScriptableObject
{
    [Header("Visuals / Audio")]
    public Sprite sprite;
    public Color color = Color.white;
    public string spawnSfx;
    public string mergeSfx;

    [Header("Base Value (used if you don't override at spawn)")]
    [Min(1)] public int value = 1;

    [Header("Color Identity & Rarity")]
    [Tooltip("Blocks that share this key will MERGE together. Ex: red, green, blue, purple...")]
    public string colorMerge;
    [Tooltip("Higher = more common in spawns. Tune per color (e.g., purple lower if you want it rarer).")]
    [Min(0)] public int spawnWeight = 10;

    [Header("Scoring Weight (Color-Sum mode)")]
    [Tooltip("Common colors < 1 (fewer points), rare colors > 1 (more points).")]
    [Range(0.25f, 2f)] public float frequencyScoreFactor = 1f;
}