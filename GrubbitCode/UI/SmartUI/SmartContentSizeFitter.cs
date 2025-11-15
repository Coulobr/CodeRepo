using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Grubbit
{
    [AddComponentMenu("Layout/Smart Content Size Fitter", 142)]
    [ExecuteInEditMode]
    public class SmartContentSizeFitter : ContentSizeFitter
    {
        #region Variables
        public bool hasMinHorizontal;
        public int minHorizontal;
        public bool hasMaxHorizontal;
        public int maxHorizontal;

        public bool hasMinVertical;
        public int minVertical;
        public bool hasMaxVertical;
        public int maxVertical;

        // Unity is dumb and has the content size fitter's rectTransform variable set to private so
        // we can't inherit it here
        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }
        #endregion

        #region Smart Content Size Fitter Functions
        public override void SetLayoutHorizontal()
        {
            base.SetLayoutHorizontal();

            var horizontalSize = 0f;
            switch (horizontalFit)
            {
                case FitMode.PreferredSize:
                    horizontalSize = LayoutUtility.GetPreferredSize(rectTransform, 0);
                    break;
                case FitMode.MinSize:
                    horizontalSize = LayoutUtility.GetMinSize(rectTransform, 0);
                    break;
                case FitMode.Unconstrained:
                    return;
            }

            if (hasMinHorizontal)
            {
                horizontalSize = Mathf.Max(horizontalSize, minHorizontal);
            }

            if (hasMaxHorizontal)
            {
                horizontalSize = Mathf.Min(horizontalSize, maxHorizontal);
            }

            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)0, horizontalSize);
        }

        public override void SetLayoutVertical()
        {
            base.SetLayoutVertical();

            var verticalSize = 0f;
            switch (verticalFit)
            {
                case FitMode.PreferredSize:
                    verticalSize = LayoutUtility.GetPreferredSize(rectTransform, 1);
                    break;
                case FitMode.MinSize:
                    verticalSize = LayoutUtility.GetMinSize(rectTransform, 1);
                    break;
                case FitMode.Unconstrained:
                    return;
            }

            if (hasMinVertical)
            {
                verticalSize = Mathf.Max(verticalSize, minVertical);
            }

            if (hasMaxVertical)
            {
                verticalSize = Mathf.Min(verticalSize, maxVertical);
            }

            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)1, verticalSize);
        }
        #endregion
    }
}
