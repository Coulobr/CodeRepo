using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBlockPool : MonoBehaviour
{
    public static UIBlockPool Instance { get; private set; }

    [SerializeField] private UIBlockRuntime prefab;
    [SerializeField] private RectTransform parentForBlocks;

    private readonly Stack<UIBlockRuntime> _pool = new();

    void Awake()
    {
        Instance = this;
        if (!parentForBlocks) parentForBlocks = GetComponent<RectTransform>();
    }

    public UIBlockRuntime Get()
    {
        UIBlockRuntime b;
        if (_pool.Count > 0)
        {
            b = _pool.Pop();
            b.gameObject.SetActive(true);
        }
        else
        {
            b = Instantiate(prefab, parentForBlocks, false);
        }
        return b;
    }

    public void Release(UIBlockRuntime b)
    {
        b.gameObject.SetActive(false);
        b.transform.SetParent(parentForBlocks, false);
        _pool.Push(b);
    }
}