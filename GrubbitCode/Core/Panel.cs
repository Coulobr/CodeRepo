using UnityEngine;
using DG.Tweening;

namespace Grubbit
{
    /// <summary>
    /// Panels are UI subobjects that are generally attached to a parent Menu object. 
    /// Panels will generally have a canvas but are not always required to have one.
    /// Panels can be directly attached to a Menu and then referenced in the prefab directly,
    /// or can be created at runtime.
    /// Panels are found and created from the Resources/UI/Panels folder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Panel<T> : Panel where T : Panel<T>
    {
        /// <summary>
        /// Finds the related prefab in the Resources/UI/Panels folder, and returns the instantiated gameobject.
        /// </summary>
        public static T Create(Transform parent = null)
        {
            var thisType = typeof(T);
            var panelResource = Resources.Load($"UI_Panels/{thisType.Name}", thisType);

            if (!panelResource)
            {
                Debug.LogError("There is no panel prefab in the Resources folder, cannot instantiate the panel.");
                return null;
            }

            var newPanel = Instantiate(panelResource, parent) as T;

            if (newPanel != null)
            {
                newPanel.gameObject.name = newPanel.name.Replace("(Clone)", "");
                return newPanel;
            }

            return null;
        }

        protected override void OnOpened() { }
        protected override void OnClosed() { }
        protected override void InternalUpdate() { }
    }

    public abstract class Panel : MonoBehaviour
    {
        [Header("Open/Close Related Settings")] public GameObject itemToTween;
        public bool animateOnOpen;
        public bool fireOnOpenedAfterAnimation;
        public float openAnimateSpeed = 0.33f;
        public Vector3 openFinalScale = Vector3.one;
        public Ease openEaseType = Ease.OutBack;
        public bool animateOnClose;
        public float closeAnimateSpeed = 0.33f;
        public Vector3 closeFinalScale = Vector3.zero;
        public Ease closeEaseType = Ease.InBack;
        protected Tween animationTween;

        [Header("Other Settings")] public bool startOpen;
        [HideInInspector] public bool isOpen;

        protected virtual void Awake()
        {
            if (startOpen)
            {
                Open();
            }
            else
            {
                gameObject.SetActive(false);
                isOpen = false;
            }
        }

        protected virtual void Update()
        {
            if (isOpen)
            {
                InternalUpdate();
            }
        }

        public void Open(bool skipAnimation = false)
        {
            if (isOpen)
            {
                return;
            }

            if (animationTween != null)
            {
                animationTween.Complete();
            }

            gameObject.SetActive(true);
            isOpen = true;

            if (!fireOnOpenedAfterAnimation || skipAnimation)
            {
                OnOpened();
            }

            if (animateOnOpen && itemToTween != null && !skipAnimation)
            {
                itemToTween.transform.localScale = closeFinalScale;
                animationTween = itemToTween.transform.DOScale(openFinalScale, openAnimateSpeed).SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        if (fireOnOpenedAfterAnimation)
                        {
                            OnOpened();
                        }

                        animationTween = null;
                    });
            }
        }

        public void Close(bool skipAnimation = false)
        {
            if (!isOpen)
            {
                return;
            }

            if (animationTween != null)
            {
                animationTween.Complete();
            }

            if (animateOnClose && itemToTween != null && !skipAnimation)
            {
                animationTween = itemToTween.transform.DOScale(closeFinalScale, closeAnimateSpeed)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        isOpen = false;
                        OnClosed();
                        animationTween = null;
                    });
            }
            else
            {
                gameObject.SetActive(false);
                isOpen = false;
                OnClosed();
            }
        }

        /// <summary>
        /// An internal function called when the panel element is opened.
        /// </summary>
        protected abstract void OnOpened();

        /// <summary>
        /// An internal function called when the panel element is closed.
        /// </summary>
        protected abstract void OnClosed();

        /// <summary>
        /// An internal function called on Update when the panel element is open.
        /// </summary>
        protected virtual void InternalUpdate() { }

        /// <summary>
        /// Manually updates elements within a panel.
        /// </summary>
        public virtual void UpdatePanel() { }

        /// <summary>
        /// Resets the panel to an original state.
        /// </summary>
        public virtual void ResetPanel() { }

        /// <summary>
        /// Sets up the panel for use, usually called after it's opened.
        /// </summary>
        public virtual void SetupPanel() { }
        
        /// <summary>
        /// Refreshes the panel, causing some select/specific contents to be updated.
        /// </summary>
        public virtual void RefreshPanel() { }

        /// <summary>
        /// Sets the panel's rect transform position to the given position
        /// </summary>
        public void SetPanelPosition(RectTransformPositionData posData)
        {
            if (posData == null)
            {
	            Debug.LogWarning("The given posData is null, cannot set the position of the rect transform...");
                return;
            }

            var rect = GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition = posData.anchorPos;
                rect.anchorMin = posData.anchorMin;
                rect.anchorMax = posData.anchorMax;
                rect.pivot = posData.pivot;
                rect.localScale = posData.localScale;
            }
            else
            {
	            Debug.LogWarning("This panel does not have a rect transform, cancelling...");
            }
        }

        protected virtual bool HasRequiredAssets()
        {
            return true;
        }
    }
}