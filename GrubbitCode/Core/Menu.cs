using System;
using UnityEngine;
using DG.Tweening;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable CommentTypo

namespace Grubbit
{
    /// <summary>
    /// A Menu is the overall UI object that contains a mix of components and panels. 
    /// Each Menu has a canvas, and child panels might also have canvases. Menus are
    /// found and created from the Resources/UI/Menus folder.
    /// </summary>
    public abstract class Menu<T> : Menu where T : Menu<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    var newMenu = Create(typeof(T));

                    if (newMenu == null)
                    {
                        return null;
                    }
                }

                return instance;
            }
        }

        public static bool IsOpen => Instance.isOpen;


        [SerializeField] private bool startOpen = false;

        private void AssignToInstance()
        {
            instance = (T) this;
            gameObject.SetActive(false);
            isOpen = false;
        }

        protected virtual void Awake()
        {
			if (instance == null)
            {
                AssignToInstance();
            }

#if UNITY_EDITOR
            if (startOpen)
            {
	            Open();
            }
#endif
		}

		protected virtual void OnDestroy()
        {
            instance = null; // Sanity check
        }

        /// <summary>
        /// Opens the menu element.
        /// </summary>
        public static Menu Open()
        {
            if (IsOpen)
            {
                return instance;
            }

            instance.Initialize();

            if (instance.animationTween != null)
            {
                instance.animationTween.Complete();
            }

            instance.InternalOpen();
            instance.transform.SetAsLastSibling();

            if (instance.animateOnOpen && instance.itemToTween != null)
            {
                if (!instance.onOpenedAfterTween)
                {
                    instance.OnOpened();
                }

                instance.itemToTween.transform.localScale = instance.closeFinalScale;
                instance.animationTween = instance.itemToTween.transform.DOScale(instance.openFinalScale, instance.openAnimateSpeed)
                    .SetEase(instance.openEaseType)
                    .OnComplete(() =>
                    {
                        instance.animationTween = null;

                        if (instance.onOpenedAfterTween)
                        {
                            instance.OnOpened();
                        }
                    });
            }
            else
            {
                instance.OnOpened();
            }

            return instance;
        }

        /// <summary>
        /// Closes the menu element.
        /// </summary>
        public static Menu Close()
        {
            if (instance == null)
            {
                return null;
            }

            if (!IsOpen)
            {
                return instance;
            }

            if (instance.animationTween != null)
            {
                instance.animationTween.Complete();
            }

            if (instance.animateOnClose && instance.itemToTween != null)
            {
                instance.animationTween = instance.itemToTween.transform.DOScale(instance.closeFinalScale, instance.closeAnimateSpeed)
                    .SetEase(instance.closeEaseType)
                    .OnComplete(() =>
                    {
                        instance.InternalClose();
                        instance.animationTween = null;
                    });
            }
            else
            {
                instance.InternalClose();
            }

            return instance;
        }

        public static Menu Create(Type desiredType)
        {
            if (desiredType == null)
            {
                Debug.LogError("Supplied Type is null.");
                return null;
            }

            var newItem = Instantiate(Resources.Load($"UIMenus/{desiredType.Name}")) as GameObject;

            if (newItem == null)
            {
                Debug.LogError($"Could not instantiate item for {desiredType.Name}.");
                return null;
            }

            var newMenu = newItem.GetComponent<Menu<T>>();

            if (newMenu == null)
            {
                Debug.LogError($"Could not get menu component for {desiredType.Name}.");
                return null;
            }

            newMenu.gameObject.name = newMenu.name.Replace("(Clone)", "");

            // We want to set the right display on the canvases when they're created
            var canvas = newMenu.gameObject.GetComponent<Canvas>();

            if (canvas != null)
            {
                canvas.targetDisplay = newMenu.desiredDisplay;
            }

            newMenu.GeneratePanels();
            DontDestroyOnLoad(newMenu);
            newMenu.AssignToInstance();
            return newMenu;
        }

        /// <summary>
        /// An internal function called when the menu element is opened.
        /// </summary>
        protected override void OnOpened() { }

        /// <summary>
        /// An internal function called when the menu element is closed.
        /// </summary>
        protected override void OnClosed() { }

        /// <summary>
        /// An internal function called when the menu element is created. This function's
        /// job is to create all panels attached to this gameobject. This function won't be needed in Unity
        /// versions 2018 and later due to the addition of nested prefabs.
        /// </summary>
        public override void GeneratePanels() { }

        /// <summary>
        /// A function that can be called by anything that will refresh certain contents of the menu, without calling
        /// <see cref="OnOpened"/> again.
        /// </summary>
        public override void Refresh() { }

        /// <summary>
        /// An internal function that is called when the menu is first opened. This function's
        /// job is to replace the default monobehavior function Start(). This is because OnOpened() will
        /// frequently require the object to be initialized or it may return a NRE.
        /// </summary>
        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            OnInitialized();
            initialized = true;
        }

        protected override void OnInitialized() { }
    }

    /// <summary>
    /// A Menu is the overall UI object that contains a mix of components and panels. Each Menu has a canvas, and child panels might also have canvases. Menus are
    /// found and created from the Resources/UI/Menus folder.
    /// </summary>
    public abstract class Menu : MonoBehaviour
    {
        public virtual void InternalOpen()
        {
            gameObject.SetActive(true);
            isOpen = true;
        }

        public virtual void InternalClose()
        {
            gameObject.SetActive(false);
            isOpen = false;
            OnClosed();
        }

        [Header("General Settings")] public bool isOpen;
        public bool isAffectedByCloseAll = true;
        public GameObject itemToTween;
        public int desiredDisplay;

        [Header("Open Tween Settings")] public bool animateOnOpen;
        public bool onOpenedAfterTween;
        public float openAnimateSpeed = 0.33f;
        public Vector3 openFinalScale = Vector3.one;
        public Ease openEaseType = Ease.OutBack;

        [Header("Close Tween Settings")] public bool animateOnClose;
        public float closeAnimateSpeed = 0.33f;
        public Vector3 closeFinalScale = Vector3.zero;
        public Ease closeEaseType = Ease.InBack;

        protected Tween animationTween;
        protected bool initialized;

        public abstract void GeneratePanels();
        public abstract void Refresh();
        protected abstract void OnOpened();
        protected abstract void OnClosed();
        protected abstract void OnInitialized();
    }
}