using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ReSharper Comments
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable Unity.RedundantEventFunction
#endregion

namespace Grubbit
{
	public class MenuManager : GrubbitSingleton<MenuManager>
	{
        [Header("Generation Settings")] public bool instantGeneration = true;
        public float generationWaitTime = 0.25f;
        public bool generateOnAwake = true;

        [Header("Generation Settings")] public bool instantDestruction = true;
        public float destructionWaitTime = 0.25f;
        public float destructionDelayTime;

        [Header(("General References"))] public Camera uiCamera;

        public List<Menu> menuItems = new List<Menu>();
        protected List<Menu> createdMenuItems = new List<Menu>();
        private IEnumerator loadAllUiCr;
        private IEnumerator destroyAllUiCi;
        private bool destructionInProgress;
        private bool uiExists;

        protected override void Awake()
        {
            base.Awake();
            if (generateOnAwake)
            {
                LoadAllUI();
            }
        }

        protected override void OnDestroy()
        {
	       base.OnDestroy();
        }

		private void LoadAllUI()
        {
            if (loadAllUiCr == null)
            {
                loadAllUiCr = Co_LoadAllUi(instantGeneration ? 0f : generationWaitTime);
                StartCoroutine(loadAllUiCr);
            }
        }

        protected virtual IEnumerator Co_LoadAllUi(float waitTime)
        {
            if (destroyAllUiCi != null)
            {
                // If we're in the process of destroying the UI elements, but the
                // delay timer has not ended it's not too late to just cancel the coroutine
                if (!destructionInProgress)
                {
                    StopCoroutine(destroyAllUiCi);
                    destroyAllUiCi = null;
                }
                else
                {
                    // Wait until the destroy function is done
                    while (destroyAllUiCi != null)
                    {
                        yield return null;
                    }
                }
            }

            if (uiExists)
            {
                // Since all of the UI elements still exist, just exit
                loadAllUiCr = null;
                yield break;
            }

            var waitForSeconds = new WaitForSeconds(waitTime);

            foreach (var menu in menuItems)
            {
                var newMenu = Instantiate(menu);
                newMenu.gameObject.name = newMenu.name.Replace("(Clone)", "");

                // We want to set the right display on the canvases when they're created
                var canvas = newMenu.gameObject.GetComponent<Canvas>();

                if (canvas != null)
                {
                    canvas.targetDisplay = menu.desiredDisplay;
                }

                // Remove any duplicate menus
                if (createdMenuItems.Contains(newMenu))
                {
                    Destroy(newMenu);
                }

                newMenu.GeneratePanels();
                createdMenuItems.Add(newMenu);
                DontDestroyOnLoad(newMenu);

                if (waitTime > 0)
                {
                    yield return waitForSeconds;
                }
            }

            OnUILoaded();
            CloseAllMenus();
            DontDestroyOnLoad(uiCamera);
            uiExists = true;
            loadAllUiCr = null;
        }

        public virtual void DestroyAllUI()
        {
            if (destroyAllUiCi == null)
            {
                destructionInProgress = false;
                destroyAllUiCi = DestroyAllAUICR(instantDestruction ? 0f : destructionWaitTime);
                StartCoroutine(destroyAllUiCi);
            }
        }

        protected virtual IEnumerator DestroyAllAUICR(float waitTime)
        {
            // Wait until the destroy function is done
            while (loadAllUiCr != null)
            {
                yield return null;
            }

            if (!uiExists)
            {
                // Since all of the UI elements still exist, just exit
                destroyAllUiCi = null;
                yield break;
            }

            if (destructionDelayTime > 0)
            {
                yield return new WaitForSeconds(destructionDelayTime);
            }

            var waitForSeconds = new WaitForSeconds(waitTime);
            destructionInProgress = true;

            for (var i = createdMenuItems.Count - 1; i >= 0; --i)
            {
                Destroy(createdMenuItems[i].gameObject);

                if (waitTime > 0)
                {
                    yield return waitForSeconds;
                }
            }

            createdMenuItems.Clear();
            OnUIDestroyed();
            uiExists = false;
            destructionInProgress = false;
            destroyAllUiCi = null;
        }

        public virtual void CloseAllMenus()
        {
            for (var i = 0; i < menuItems.Count; ++i)
            {
                if (createdMenuItems[i].isOpen && createdMenuItems[i].isAffectedByCloseAll)
                {
                    createdMenuItems[i].InternalClose();
                }
            }

            OnAllUIClosed();
        }

        protected virtual void OnUILoaded() { }
        protected virtual void OnUIDestroyed() { }
        protected virtual void OnAllUIClosed() { }
    }
}