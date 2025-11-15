using System.Collections.Generic;
using UnityEngine;

public class SfxSystem : MonoBehaviour
{
    public static SfxSystem Instance { get; private set; }

    [SerializeField] private SfxLibrarySO library;
    [SerializeField] private int voices = 10;

    private readonly List<AudioSource> _sources = new();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < voices; i++)
        {
            var aGo = new GameObject($"SFX_{i}");
            aGo.transform.SetParent(transform, false);
            var a = aGo.AddComponent<AudioSource>();
            a.playOnAwake = false;
            _sources.Add(a);
        }
    }

    public void Play(string key, Vector3 pos)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!library || !library.TryGet(key, out var clip, out var vol) || clip == null) return;
        var src = GetFreeSource();
        src.transform.position = pos;
        src.volume = vol;
        src.clip = clip;
        src.Play();
    }

    private AudioSource GetFreeSource()
    {
        foreach (var s in _sources) if (!s.isPlaying) return s;
        return _sources[0];
    }
}