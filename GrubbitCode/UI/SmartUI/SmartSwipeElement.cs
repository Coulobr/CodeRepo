using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Grubbit
{
    public class SmartSwipeElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Base Settings")] public RectTransform foregroundRect;
        public float finishSwipeTweenLength = 0.5f;

        public delegate void OnDragStarted();
        public event OnDragStarted onDragStarted;

        public delegate void OnDragEnded();
        public event OnDragEnded onDragEnded;

        public delegate void OnDragPerformed();
        public event OnDragPerformed onDragPerformed;

        [Header("Left Swipe Settings")] public bool canSwipeLeft;
        public RectTransform leftSwipeBackgroundRect;
        [Range(0, 1)] public float leftSwipePercentageThreshold;

        public delegate void OnLeftSwipeCompleted();
        public event OnLeftSwipeCompleted onLeftSwipeCompleted;

        [Header("Right Swipe Settings")] public bool canSwipeRight;
        public RectTransform rightSwipeBackgroundRect;
        [Range(0, 1)] public float rightSwipePercentageThreshold;

        public delegate void OnRightSwipeCompleted();
        public event OnRightSwipeCompleted onRightSwipeCompleted;

        //private bool isDragging;
        private Vector2 initialGrabPos;
        private float foregroundWidth;
        private Tween finishTween;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!canSwipeLeft && !canSwipeRight)
            {
                return;
            }

            foregroundWidth = foregroundRect.GetWidth();
            initialGrabPos = eventData.position;

            if (canSwipeLeft)
            {
                SetLeftSwipeBackgroundWidth(0f);
            }
            else if (canSwipeRight)
            {
                SetRightSwipeBackgroundWidth(0f);
            }

            onDragStarted?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!canSwipeLeft && !canSwipeRight)
            {
                return;
            }

            var swipeDifference = eventData.position.x - initialGrabPos.x;

            if (swipeDifference < 0)
            {
                if (canSwipeLeft)
                {
                    var newForegroundPos = Vector2.zero;
                    newForegroundPos.x = Mathf.Clamp(0f - (initialGrabPos.x - eventData.position.x), -foregroundWidth / 2f, 0f);
                    foregroundRect.anchoredPosition = newForegroundPos;
                    var desiredWidth = Mathf.Clamp(0f - newForegroundPos.x, 0f, foregroundWidth);
                    SetLeftSwipeBackgroundWidth(desiredWidth);
                }

                SetRightSwipeBackgroundWidth(0f);
            }
            else if (swipeDifference >= 0)
            {
                if (canSwipeRight)
                {
                    var newForegroundPos = Vector2.zero;
                    newForegroundPos.x = Mathf.Clamp(0f + (eventData.position.x - initialGrabPos.x), 0f, 0f + foregroundWidth);
                    foregroundRect.anchoredPosition = newForegroundPos;
                    var desiredWidth = Mathf.Clamp(newForegroundPos.x - 0f, 0f, foregroundWidth);
                    SetRightSwipeBackgroundWidth(desiredWidth);
                }

                SetLeftSwipeBackgroundWidth(0f);
            }

            onDragPerformed?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!canSwipeLeft && !canSwipeRight)
            {
                return;
            }

            var swipeDifference = eventData.position.x - initialGrabPos.x;

            if (swipeDifference < 0f)
            {
                if (canSwipeLeft && leftSwipeBackgroundRect)
                {
                    var leftSwipeWidth = leftSwipeBackgroundRect.GetWidth();

                    if (leftSwipeWidth >= foregroundWidth * leftSwipePercentageThreshold)
                    {
                        FinishDeleteTween();
                        var timeUntilTweenFinishes = finishSwipeTweenLength * (1 - leftSwipeWidth / foregroundWidth);
                        finishTween = DOTween.To(() => leftSwipeWidth, x => leftSwipeWidth = x, foregroundWidth, timeUntilTweenFinishes)
                            .OnUpdate(() =>
                            {
                                var newForegroundPos = Vector2.zero;
                                newForegroundPos.x = Mathf.Clamp(0f - leftSwipeWidth, -foregroundWidth / 2f, 0f);
                                foregroundRect.anchoredPosition = newForegroundPos;
                                var desiredWidth = Mathf.Clamp(0f - newForegroundPos.x, 0f, foregroundWidth);
                                SetLeftSwipeBackgroundWidth(desiredWidth);
                            })
                            .OnComplete(() =>
                            {
                                onLeftSwipeCompleted?.Invoke();
                                finishTween = null;
                            }).Play();
                    }
                    else
                    {
                        SetLeftSwipeBackgroundWidth(0f);
                        foregroundRect.anchoredPosition = Vector2.zero;
                    }
                }

                SetRightSwipeBackgroundWidth(0f);
            }
            else if (swipeDifference > 0f)
            {
                if (canSwipeRight && rightSwipeBackgroundRect)
                {
                    var rightSwipeWidth = rightSwipeBackgroundRect.GetWidth();

                    if (rightSwipeWidth >= foregroundWidth * rightSwipePercentageThreshold)
                    {
                        FinishDeleteTween();
                        var timeUntilTweenFinishes = finishSwipeTweenLength * (1 - rightSwipeWidth / foregroundWidth);
                        finishTween = DOTween.To(() => rightSwipeWidth, x => rightSwipeWidth = x, foregroundWidth, timeUntilTweenFinishes)
                            .OnUpdate(() =>
                            {
                                var newForegroundPos = Vector2.zero;
                                newForegroundPos.x = Mathf.Clamp(0f + rightSwipeWidth, 0f, 0f + foregroundWidth);
                                foregroundRect.anchoredPosition = newForegroundPos;
                                var desiredWidth = Mathf.Clamp(newForegroundPos.x - 0f, 0f, foregroundWidth);
                                SetRightSwipeBackgroundWidth(desiredWidth);
                            })
                            .OnComplete(() =>
                            {
                                onRightSwipeCompleted?.Invoke();
                                finishTween = null;
                            }).Play();
                    }
                    else
                    {
                        SetRightSwipeBackgroundWidth(0f);
                        foregroundRect.anchoredPosition = Vector2.zero;
                    }
                }

                SetLeftSwipeBackgroundWidth(0f);
            }
            else
            {
                SetLeftSwipeBackgroundWidth(0f);
                SetRightSwipeBackgroundWidth(0f);
                foregroundRect.anchoredPosition = Vector2.zero;
            }

            onDragEnded?.Invoke();
        }

        private void FinishDeleteTween()
        {
            if (finishTween != null)
            {
                finishTween.Complete();
                finishTween = null;
            }
        }

        private void SetLeftSwipeBackgroundWidth(float width)
        {
            if (leftSwipeBackgroundRect)
            {
                leftSwipeBackgroundRect.SetWidth(width);
            }
        }

        private void SetRightSwipeBackgroundWidth(float width)
        {
            if (rightSwipeBackgroundRect)
            {
                rightSwipeBackgroundRect.SetWidth(width);
            }
        }

        public void ResetElement(bool resetEvents)
        {
            finishTween.Pause();
            finishTween = null;
            SetLeftSwipeBackgroundWidth(0f);
            SetRightSwipeBackgroundWidth(0f);
            foregroundRect.anchoredPosition = Vector2.zero;

            if (resetEvents)
            {
                onDragStarted = null;
                onDragPerformed = null;
                onDragEnded = null;
                onLeftSwipeCompleted = null;
                onRightSwipeCompleted = null;
            }
        }
    }
}