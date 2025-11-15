using System.Collections;
using System.Collections.Generic;
using HitTrax;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Grubbit
{
    public class TooltipItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public bool onHover = true;
        public bool onTap = true;
        public GrubbitEnums.TooltipType tooltipType;
        public bool animateTooltip = false;
        public string desiredBaseText;
        public string desiredTitleText;
        public string desiredSubtitleText;
        public Sprite desiredItemIcon;
        public Color desiredIconColor;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!onHover)
            {
                return;
            }

            if (!Tooltip.Instance.ToolTipActive && Tooltip.Instance.selectedItem == null)
            {
                Tooltip.ShowTooltip(this);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!onTap)
            {
                return;
            }

            if (!Tooltip.Instance.ToolTipActive && Tooltip.Instance.selectedItem == null)
            {
                Tooltip.ShowTooltip(this, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!onHover)
            {
                return;
            }

            if (Tooltip.Instance.ToolTipActive && Tooltip.Instance.selectedItem == this)
            {
                Tooltip.HideTooltip();
            }
        }
    }
}