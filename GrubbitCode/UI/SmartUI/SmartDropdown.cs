using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

#region Resharper Comments
// ReSharper disable FieldCanBeMadeReadOnly.Global
#endregion

namespace Grubbit
{
    public class SmartDropdown : TMP_Dropdown
    {
        public Image dropdownArrow;
        public Sprite standardDropdownArrowState;
        public Color standardDropdownArrowColor;
        public Sprite pressedDropdownArrowState;
        public Color pressedDropdownArrowColor;
        public Sprite disabledDropdownArrowState;
        public Color disabledDropdownArrowColor;

        public bool uniqueDisabledText;
        public TextMeshProUGUI disabledText;

        public bool startWithNoOptionSelected;
        public TextMeshProUGUI placeholderText;
        public TextMeshProUGUI subtitleText;

        public bool testStateInEditor;
        private bool currentlyShowingPlaceholderText;

        public bool CurrentlyShowingPlaceholderText
        {
            get
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    return testStateInEditor;
                }

                return currentlyShowingPlaceholderText;
            }

            private set { currentlyShowingPlaceholderText = value; }
        }

        public float childrenHeight;
        public float bufferHeight;
        public bool autoTranslate;

        public Action OnPlaceholderTextEnabled;
        public Action OnPlaceholderTextDisabled;
        public Action OnDropdownSelected;
        public Action OnDropdownSelected_PointerUp;

        protected List<string> originalOptionTexts = new List<string>();
        protected string originalPlaceholderText;
        protected string originalSubtitleText;
        protected bool initialized;
        private IEnumerator objectHeightCR;

        protected override void Awake()
        {
            if (!initialized && autoTranslate)
            {
                if (placeholderText)
                {
                    originalPlaceholderText = placeholderText.text;
                }

                if (subtitleText)
                {
                    originalSubtitleText = subtitleText.text;
                }

                for (var i = 0; i < options.Count; ++i)
                {
                    originalOptionTexts.Add(options[i].text);
                }

                if (standardDropdownArrowState == null)
                {
                    standardDropdownArrowState = dropdownArrow.sprite;
                }

                initialized = true;
                UpdateTextLanguage();
            }

            base.Awake();

            if (startWithNoOptionSelected)
            {
                EnablePlaceholderText(false);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (initialized && autoTranslate)
            {
                UpdateTextLanguage();
            }
        }

        protected void Update()
        {
            if (!interactable && disabledDropdownArrowState != null)
            {
                dropdownArrow.sprite = disabledDropdownArrowState;
                dropdownArrow.color = disabledDropdownArrowColor;
            }
            else
            {
                if (IsExpanded && pressedDropdownArrowState != null)
                {
                    dropdownArrow.sprite = pressedDropdownArrowState;
                    dropdownArrow.color = pressedDropdownArrowColor;
                }
                else if (standardDropdownArrowState != null)
                {
                    dropdownArrow.sprite = standardDropdownArrowState;
                    dropdownArrow.color = standardDropdownArrowColor;
                }
            }

            if (placeholderText != null)
            {
                if (CurrentlyShowingPlaceholderText)
                {
                    if (!placeholderText.gameObject.activeSelf)
                    {
                        placeholderText.gameObject.SetActive(true);
                    }

                    if (disabledText != null && disabledText.gameObject.activeSelf)
                    {
                        disabledText.gameObject.SetActive(false);
                    }

                    if (captionText != null && captionText.gameObject.activeSelf)
                    {
                        captionText.gameObject.SetActive(false);
                    }

                    return;
                }

                if (placeholderText.gameObject.activeSelf)
                {
                    placeholderText.gameObject.SetActive(false);
                }
            }

            if (uniqueDisabledText && disabledText != null)
            {
                if (interactable)
                {
                    if (disabledText.gameObject.activeSelf)
                    {
                        disabledText.gameObject.SetActive(false);
                    }

                    if (!captionText.gameObject.activeSelf)
                    {
                        captionText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (!disabledText.gameObject.activeSelf)
                    {
                        disabledText.gameObject.SetActive(true);
                    }

                    if (captionText.gameObject.activeSelf)
                    {
                        captionText.gameObject.SetActive(false);
                    }
                }
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (interactable)
            {
                DisablePlaceholderText(true);
            }

            OnDropdownSelected?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            OnDropdownSelected_PointerUp?.Invoke();
        }

        public void UpdateTextLanguage()
        {
            if (!autoTranslate)
            {
                return;
            }

            for (var i = 0; i < options.Count; ++i)
            {
                options[i].text = Localization.GetTranslation(originalOptionTexts[i]);
            }

            if (placeholderText)
            {
                placeholderText.text = Localization.GetTranslation(originalPlaceholderText);
            }

            if (subtitleText)
            {
                subtitleText.text = Localization.GetTranslation(originalSubtitleText);
            }
        }

        protected override GameObject CreateDropdownList(GameObject goTemplate)
        {
            var desiredHeight = options.Count * childrenHeight + bufferHeight;
            goTemplate.GetComponent<RectTransform>().SetHeight(desiredHeight);
            var dropdown = base.CreateDropdownList(goTemplate);
            UpdateObjectHeight(dropdown, desiredHeight);
            return dropdown;
        }

        public void EnablePlaceholderText(bool invokeAction)
        {
            CurrentlyShowingPlaceholderText = true;
            Utility.SetActive(placeholderText, true);
            Utility.SetActive(captionText, false);

            if (invokeAction)
            {
                OnPlaceholderTextEnabled?.Invoke();
                OnPlaceholderTextEnabled = null;
            }
        }

        public void DisablePlaceholderText(bool invokeAction = false)
        {
            CurrentlyShowingPlaceholderText = false;
            Utility.SetActive(placeholderText, false);
            Utility.SetActive(captionText, true);

            if (invokeAction)
            {
                OnPlaceholderTextDisabled?.Invoke();
                OnPlaceholderTextDisabled = null;
            }
        }
        
        private void UpdateObjectHeight(GameObject dropdown, float desiredHeight)
        {
            if (objectHeightCR != null)
            {
                StopCoroutine(objectHeightCR);
                objectHeightCR = null;
            }

            objectHeightCR = UpdateObjectHeightCR(dropdown, desiredHeight);
            StartCoroutine(objectHeightCR);
        }

        private IEnumerator UpdateObjectHeightCR(GameObject dropdown, float desiredHeight)
        {
            yield return null;
            yield return null;
            dropdown.GetComponent<RectTransform>().SetHeight(desiredHeight);
        }
    }
}