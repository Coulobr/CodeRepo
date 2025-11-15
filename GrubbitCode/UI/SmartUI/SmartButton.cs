using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Grubbit
{
    public class SmartButton : Button
	{
		private static Color HoverTint => new Color(225 / 255f, 225 / 255f, 225 / 255f, 1);

		public bool holdMode;
        public bool toggleMode;
        public bool incrementMode;

        // ========== HOLD MODE / INCREMENT MODE VARIABLES ========== \\
        public bool holding;
        public bool hovering;
        public UnityEvent onBeginHold;
        public UnityEvent onHold;
        public UnityEvent onEndHold;
        public UnityEvent onIncrementHold;
        public float holdIncrement = 0.25f;
        public float incrementMultiplier;
        private float waitTime;
        private float doubleWaitTime;
        private float tripleWaitTime;
        private float timeBeforeDouble = 2f;
        private bool doubleIncrement;
        private float timeBeforeTriple = 4f;
        private bool tripleIncrement;

        private float currentIncrement;
        // ========================================================== \\

        // ================== TOGGLE MODE VARIABLES ================== \\
        public bool setInitialToggleState;
        public bool initialToggleState;
        public bool autoToggleOnPress;
        public List<SmartButtonToggleItem> toggleItems = new List<SmartButtonToggleItem>();

        public Action<PointerEventData> OnToggle_Activated;
        public Action<PointerEventData> OnToggle_Deactivated;
        public Action<PointerEventData> OnHover_Enter;
        public Action<PointerEventData> OnHover_Exit;
        public Action<bool> OnToggled;
        public bool testStateInEditor;
        private bool currentlyEnabled = true;

        private bool pointerDownCoolingDown;
        private bool pointerUpCoolingDown;
        private float pointerUpCooldownLeft;
        private float pointerDownCooldownLeft;
        private float cooldownTime = 0.1f;

        public bool CurrentlyEnabled
        {
            get
            {
                //if (Application.isEditor && !Application.isPlaying)
                //{
                //    return testStateInEditor;
                //}

                return currentlyEnabled;
            }

            private set { currentlyEnabled = value; }
        }

        public bool tweenOnHover;
        public float tweenHoverMagnitude;
        public List<RectTransform> hoverTweenRects;
        public Image hoverTintImageTarget;
        // ========================================================== \\

        protected override void Awake()
        {
            currentlyEnabled = setInitialToggleState && initialToggleState;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if (setInitialToggleState)
            {
                ToggleState(initialToggleState);
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            ToggleState(CurrentlyEnabled, false);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
	        if (eventData == null)
	        {
		        return;
	        }

	        base.OnPointerEnter(eventData);
	        if (!interactable)
	        {
                return;
	        }

	        if (CurrentlyEnabled)
	        {
		        OnHoverTween(true);
				OnHoverTint(true);
		        hovering = true;
				OnHover_Enter?.Invoke(eventData);
	        }
		}

        public override void OnPointerExit(PointerEventData eventData)
        {
	        if (eventData == null)
	        {
		        return;
	        }

			base.OnPointerEnter(eventData);
	        if (!interactable)
	        {
		        return;
	        }

	        if (CurrentlyEnabled)
			{
				OnHoverTween(false);
				OnHoverTint(false);
				hovering = false;
				OnHover_Exit?.Invoke(eventData);
	        }
        }

		public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!interactable || pointerDownCoolingDown)
            {
                return;
            }

            holding = true;

            if (holdMode)
            {
                onBeginHold?.Invoke();
            }

            if (incrementMode)
            {
                onIncrementHold?.Invoke();
                waitTime = 0f;
                doubleWaitTime = 0f;
                tripleWaitTime = 0f;
                incrementMultiplier = 1;
            }

            pointerDownCooldownLeft = 0f;
            pointerDownCoolingDown = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (!interactable || pointerUpCoolingDown)
            {
                return;
            }

            holding = false;

            if (holdMode)
            {
                onEndHold?.Invoke();
            }

            if (incrementMode)
            {
                doubleIncrement = false;
                tripleIncrement = false;
            }

            if (autoToggleOnPress)
            {
                ToggleState(!CurrentlyEnabled);
            }

            // Determine whether we want to fire the enabled, or disabled code
            if (CurrentlyEnabled)
            {
                OnToggle_Activated?.Invoke(eventData);
            }
            else
            {
                OnToggle_Deactivated?.Invoke(eventData);
            }

            pointerUpCooldownLeft = 0f;
            pointerUpCoolingDown = true;
        }

        protected void Update()
        {
            if (pointerDownCoolingDown)
            {
                if (pointerDownCooldownLeft >= cooldownTime)
                {
                    pointerDownCooldownLeft = 0f;
                    pointerDownCoolingDown = false;
                }
                else
                {
                    pointerDownCooldownLeft += Time.deltaTime;
                }
            }

            if (pointerUpCoolingDown)
            {
                if (pointerUpCooldownLeft >= cooldownTime)
                {
                    pointerUpCooldownLeft = 0f;
                    pointerUpCoolingDown = false;
                }
                else
                {
                    pointerUpCooldownLeft += Time.deltaTime;
                }
            }

            if (holding)
            {
                if (holdMode)
                {
                    onHold?.Invoke();
                }

                if (incrementMode)
                {
                    if (waitTime >= holdIncrement / incrementMultiplier)
                    {
                        onIncrementHold?.Invoke();
                        waitTime = 0f;
                    }
                    else
                    {
                        waitTime += Time.deltaTime;
                    }

                    if (!doubleIncrement)
                    {
                        if (doubleWaitTime >= timeBeforeDouble)
                        {
                            doubleIncrement = true;
                            incrementMultiplier = 2f;
                        }
                        else
                        {
                            doubleWaitTime += Time.deltaTime;
                        }
                    }

                    if (!tripleIncrement)
                    {
                        if (tripleWaitTime >= timeBeforeTriple)
                        {
                            tripleIncrement = true;
                            incrementMultiplier = 4f;
                        }
                        else
                        {
                            tripleWaitTime += Time.deltaTime;
                        }
                    }
                }
            }
        }


        private void OnHoverTween(bool isPointerEnter)
        {
	        if (hoverTweenRects is not { Count: > 0 }) return;

	        foreach (var rect in hoverTweenRects)
	        {
		        rect.DOScale(isPointerEnter ? tweenHoverMagnitude : 1.0f, .33f).SetEase(Ease.OutBack);
	        }
        }

        private void OnHoverTint(bool isPointerEnter)
        {
	        if (hoverTintImageTarget != null)
	        {
		        hoverTintImageTarget.DOColor(isPointerEnter ? HoverTint : Color.white, 0f);
	        }
        }

		public bool ToggleState(bool enable, bool fireOffActions = true)
        {
            CurrentlyEnabled = enable;

            foreach (var item in toggleItems)
            {
                switch (item.type)
                {
                    case GrubbitEnums.ToggleItemType.Image:
                        if (item.targetImage != null)
                        {
                            var desiredActiveColor = item.activeColor;
                            var desiredInactiveColor = item.inactiveColor;
                            var desiredDisabledColor = item.disabledColor;

                            if (item.overrideColorAlpha)
                            {
                                desiredActiveColor.a = item.colorAlpha;
                                desiredInactiveColor.a = item.colorAlpha;
                                desiredDisabledColor.a = item.colorAlpha;
                            }

                            if (item.shouldColor)
                            {
                                if (item.uniqueDisabledColor && !interactable)
                                {
                                    item.targetImage.color = desiredDisabledColor;
                                }
                                else
                                {
                                    item.targetImage.color = enable ? desiredActiveColor : desiredInactiveColor;
                                }
                            }

                            if (item.changeSpriteWhenStateToggled)
                            {
                                switch (currentSelectionState)
                                {
                                    case SelectionState.Normal:
                                        item.targetImage.sprite = enable ? item.normalSpriteWhenActive : item.disabledSpriteWhenInactive;
                                        break;
                                    case SelectionState.Highlighted:
                                        item.targetImage.sprite = enable ? item.highlightedSpriteWhenActive : item.highlightedSpriteWhenInactive;
										break;
                                    case SelectionState.Pressed:
                                        item.targetImage.sprite = enable ? item.pressedSpriteWhenActive : item.pressedSpriteWhenInactive;
                                        break;
                                    case SelectionState.Disabled:
                                        item.targetImage.sprite = enable ? item.disabledSpriteWhenActive : item.disabledSpriteWhenInactive;
                                        break;
                                }
                            }

                            if (item.toggleGameObjectActiveState)
                            {
                                var showImage = true;

                                if (item.showWhenActive && CurrentlyEnabled)
                                {
                                    showImage = true;
                                }
                                else if (item.hideWhenActive && CurrentlyEnabled)
                                {
                                    showImage = false;
                                }

                                if (item.showWhenInactive && !CurrentlyEnabled)
                                {
                                    showImage = true;
                                }
                                else if (item.hideWhenInactive && !CurrentlyEnabled)
                                {
                                    showImage = false;
                                }

                                if (item.showWhenDisabled && !interactable)
                                {
                                    showImage = true;
                                }
                                else if (item.hideWhenDisabled && !interactable)
                                {
                                    showImage = false;
                                }

                                item.targetImage.gameObject.SetActive(showImage);
                            }

                            if (item.moveAnchorPos && item.rectTransform != null)
                            {
                                if (item.animateAnchorPosMovement && Application.isPlaying)
                                {
                                    for (var i = 0; i < item.animateTweens.Count; ++i)
                                    {
                                        item.animateTweens[i].Complete();
                                    }

                                    item.animateTweens.Clear();
                                    item.animateTweens.Add(item.rectTransform.DOAnchorMax(enable ? item.anchorMaxActive : item.anchorMaxInactive, item.animSpeed));
                                    item.animateTweens.Add(item.rectTransform.DOAnchorMin(enable ? item.anchorMinActive : item.anchorMinInactive, item.animSpeed));
                                    item.animateTweens.Add(item.rectTransform.DOPivot(enable ? item.pivotActive : item.pivotInactive, item.animSpeed));
                                    item.animateTweens.Add(item.rectTransform.DOAnchorPos(enable ? item.anchorPosActive : item.anchorPosInactive, item.animSpeed));
                                }
                                else
                                {
                                    item.rectTransform.anchorMin = enable ? item.anchorMinActive : item.anchorMinInactive;
                                    item.rectTransform.anchorMax = enable ? item.anchorMaxActive : item.anchorMaxInactive;
                                    item.rectTransform.pivot = enable ? item.pivotActive : item.pivotInactive;
                                    item.rectTransform.anchoredPosition = enable ? item.anchorPosActive : item.anchorPosInactive;
                                }
                            }
                        }

                        break;
                    case GrubbitEnums.ToggleItemType.TextMeshProUGUI:
                        if (item.targetTMPText != null)
                        {
                            if (item.shouldColor)
                            {
                                var desiredActiveColor = item.activeColor;
                                var desiredInactiveColor = item.inactiveColor;
                                var desiredDisabledColor = item.disabledColor;

                                if (item.overrideColorAlpha)
                                {
                                    desiredActiveColor.a = item.colorAlpha;
                                    desiredInactiveColor.a = item.colorAlpha;
                                    desiredDisabledColor.a = item.colorAlpha;
                                }

                                if (item.uniqueDisabledColor && !interactable)
                                {
                                    item.targetTMPText.color = desiredDisabledColor;
                                }
                                else
                                {
                                    item.targetTMPText.color = enable ? desiredActiveColor : desiredInactiveColor;
                                }
                            }

                            if (item.hideWhenActive)
                            {
                                item.targetTMPText.gameObject.SetActive(!CurrentlyEnabled);
                            }
                            else if (item.hideWhenInactive)
                            {
                                item.targetTMPText.gameObject.SetActive(CurrentlyEnabled);
                            }

                            if (item.changeText)
                            {
                                if (item.uniqueDisabledText && !interactable)
                                {
                                    item.targetTMPText.text = item.textWhenDisabled;
                                }
                                else
                                {
                                    item.targetTMPText.text = enable ? item.textWhenActive : item.textWhenInactive;
                                }
                            }
                        }

                        break;
                    case GrubbitEnums.ToggleItemType.SmartButton:
                        if (item.targetSmartButton != null)
                        {
                            if (item.changeTargetSmartButtonStateWhenThisActivated && enable)
                            {
                                item.targetSmartButton.ToggleState(item.targetSmartButtonStateWhenThisActive);
                            }

                            if (item.changeTargetSmartButtonStateWhenThisDeactivated && !enable)
                            {
                                item.targetSmartButton.ToggleState(item.targetSmartButtonStateWhenThisInactive);
                            }
                        }

                        break;
                }
            }

            if (fireOffActions)
            {
                OnToggled?.Invoke(enable);
            }

            return enable;
        }

        public void ForceEditorRefresh()
        {
            if (Application.isEditor)
            {
                DoStateTransition(currentSelectionState, true);
            }
        }
    }
}