using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MergeTrix/SFX Library", fileName = "SfxLibrary")]
public class SfxLibrarySO : ScriptableObject
{
    [System.Serializable]
    public struct Entry { public string key; public AudioClip clip; public float volume; }
    public List<Entry> clips = new();

    public bool TryGet(string key, out AudioClip clip, out float vol)
    {
        foreach (var e in clips)
        {
            if (e.key == key) { clip = e.clip; vol = e.volume <= 0 ? 1f : e.volume; return true; }
        }
        clip = null; vol = 1f; return false;
    }
}