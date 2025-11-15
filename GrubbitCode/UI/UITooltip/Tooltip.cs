using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Grubbit
{
    public class Tooltip : MonoBehaviour
    {
        public static Tooltip Instance { get; private set; }
        public bool ToolTipActive => tooltipParent.activeSelf;
        public float tooltipAnimateSpeed = 0.1f;
        public float minimumActiveTime = 0.1f;
        public GameObject tooltipParent;
        [HideInInspector] public TooltipItem selectedItem;

        public List<TooltipTemplate> tooltipTemplates = new List<TooltipTemplate>();
        protected float activeFor;
        protected bool activatedByTap;
        protected Tween animateTween;

        protected void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }

            Instance = this;
            DontDestroyOnLoad(this);
            tooltipParent.SetActive(false);

            if (tooltipTemplates.Count < 1)
            {
                Debug.LogError("There are no templates to use, deleting tooltip instance");
                Destroy(this);
                return;
            }

            foreach (var tooltipTemplate in tooltipTemplates)
            {
                tooltipTemplate.gameObject.SetActive(false);
            }
        }

        protected void Update()
        {
            if (ToolTipActive)
            {
                activeFor += Time.deltaTime;

                if (selectedItem != null && selectedItem.onTap && Input.GetMouseButtonDown(0) && activeFor >= minimumActiveTime)
                {
                    HideTooltip();
                }
                else if (!activatedByTap)
                {
                    tooltipParent.transform.position = Input.mousePosition;
                }
            }
        }

        /// <summary>
        /// Activates the desired tooltip.
        /// </summary>
        public static void ShowTooltip(TooltipItem hoveredItem, bool activatedByTap = false)
        {
            if (Instance)
            {
                Instance.activatedByTap = activatedByTap;
                Instance.activeFor = 0f;
                Instance.selectedItem = hoveredItem;
                Instance.tooltipParent.SetActive(true);
                Instance.tooltipParent.transform.position = Input.mousePosition;
                var tooltipIndex = (int) hoveredItem.tooltipType;

                for (var i = 0; i < Instance.tooltipTemplates.Count; ++i)
                {
                    Instance.tooltipTemplates[i].gameObject.SetActive(i == tooltipIndex);
                }

                Utility.SetText(Instance.tooltipTemplates[tooltipIndex].baseText, hoveredItem.desiredBaseText);
                Utility.SetText(Instance.tooltipTemplates[tooltipIndex].titleText, hoveredItem.desiredTitleText);
                Utility.SetText(Instance.tooltipTemplates[tooltipIndex].subtitleText, hoveredItem.desiredSubtitleText);

                if (Instance.tooltipTemplates[tooltipIndex].itemIcon != null)
                {
                    Instance.tooltipTemplates[tooltipIndex].itemIcon.sprite = hoveredItem.desiredItemIcon;
                    Instance.tooltipTemplates[tooltipIndex].itemIcon.color = hoveredItem.desiredIconColor;
                    Instance.tooltipTemplates[tooltipIndex].itemIcon.gameObject.SetActive(Instance.tooltipTemplates[tooltipIndex].itemIcon.sprite != null);
                }

                if (hoveredItem.animateTooltip)
                {
                    Instance.tooltipParent.transform.localScale = Vector3.zero;
                    Instance.animateTween = Instance.tooltipParent.transform.DOScale(Vector3.one, Instance.tooltipAnimateSpeed)
                        .SetEase(Instance.tooltipTemplates[tooltipIndex].desiredEaseType)
                        .OnComplete(() => { Instance.animateTween = null; });
                }
                else
                {
                    Instance.tooltipParent.transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public static void HideTooltip()
        {
            if (Instance)
            {
                Instance.activeFor = 0f;
                Instance.activatedByTap = false;

                if (Instance.selectedItem.animateTooltip)
                {
                    Instance.tooltipParent.transform.localScale = Vector3.one;
                    Instance.tooltipParent.transform.DOScale(Vector3.zero, Instance.tooltipAnimateSpeed)
                        .SetEase(Ease.OutCirc)
                        .OnComplete(() =>
                        {
                            Instance.animateTween = null;
                            Instance.HideTooltipObjects();
                        });
                }
                else
                {
                    Instance.HideTooltipObjects();
                    Instance.tooltipParent.transform.localScale = Vector3.zero;
                }
            }
        }

        private void HideTooltipObjects()
        {
            tooltipParent.SetActive(false);
            selectedItem = null;

            foreach (var tooltipTemplate in tooltipTemplates)
            {
                tooltipTemplate.gameObject.SetActive(false);
            }
        }
    }
}