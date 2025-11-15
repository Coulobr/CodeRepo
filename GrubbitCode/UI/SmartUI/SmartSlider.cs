using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Grubbit
{
    public class SmartSlider : Slider
    {
        public bool autoFade;
        public float hideAfter = 3f;
        public float fadeDuration = 0.25f;
        public float showOpacity = 1f;
        public float hideOpacity;
        public CanvasGroup canvasGroup;
        public Action<float> onManuallySet;

        private bool showing;
        private float currentTimeUntilFade;
        private Tween fadeTween;
        private bool canFade;

        protected override void Start()
        {
            if (canvasGroup)
            {
                canFade = true;
            }
        }

        protected override void Update()
        {
	        base.Update();
	        if (canFade && showing && autoFade)
	        {
		        if (currentTimeUntilFade >= hideAfter)
		        {
			        Fade(false);
			        return;
		        }

		        currentTimeUntilFade += Time.deltaTime;
	        }
		}

        private void Fade(bool fadeIn)
        {
            if (!canFade) return;

            fadeTween?.Pause();
            fadeTween = null;
            fadeTween = canvasGroup.DOFade(fadeIn ? showOpacity : hideOpacity, fadeDuration)
                .OnComplete(() =>
                {
                    showing = fadeIn;
                    fadeTween = null;
                });
        }

        public void ManualSet(float desiredValue, bool triggerValueCallback)
        {
            var adjustedValue = Mathf.Clamp(desiredValue, minValue, maxValue);
            Set(adjustedValue, triggerValueCallback);
            onManuallySet?.Invoke(adjustedValue);
        }
    }
}