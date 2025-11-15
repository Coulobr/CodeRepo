using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grubbit
{
    public class GrubbitPopupUI : Menu<GrubbitPopupUI>
    {
        [Header("Grubbit Popup UI References")] public TextMeshProUGUI titleText;
        public TextMeshProUGUI informationText;
        public Button optionAButton;
        public Button optionBButton;
        public Button optionCButton;
        public Button closeButton;
        public int instanceNum = -1;

        protected override void Awake()
        {
            base.Awake();
            instanceNum = -1;
        }

        protected void Start()
        {
            if (closeButton)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => { Close(); });
            }
        }

        protected void OnDisable()
        {
            instanceNum = -1;
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            instanceNum = -1;
        }

        public GrubbitPopupUI UpdateElement(string desiredTitleText, string infoText, bool useDefaultButtonsResponse, bool showCloseButton = false, int desiredInstanceNum = 0)
        {
            if (instanceNum == desiredInstanceNum)
            {
                return null;
            }

            titleText.text = desiredTitleText;
            informationText.text = infoText;
            instanceNum = desiredInstanceNum;

            if (useDefaultButtonsResponse)
            {
                ResetButtonOptions();
            }

            closeButton.gameObject.SetActive(showCloseButton);
            return this;
        }

        public GrubbitPopupUI UpdateButtonOptions(Action onOptionAPressed = null, Action onOptionBPressed = null, Action onOptionCPressed = null, bool reopeningPopup = false)
        {
            optionAButton.onClick.RemoveAllListeners();
            optionBButton.onClick.RemoveAllListeners();
            optionCButton.onClick.RemoveAllListeners();
            optionAButton.gameObject.SetActive(onOptionAPressed != null);
            optionBButton.gameObject.SetActive(onOptionBPressed != null);
            optionCButton.gameObject.SetActive(onOptionCPressed != null);

            if (onOptionAPressed != null)
            {
                optionAButton.onClick.AddListener(() =>
                {
                    onOptionAPressed();

                    if (!reopeningPopup)
                    {
                        Close();
                    }
                });
            }

            if (onOptionBPressed != null)
            {
                optionBButton.onClick.AddListener(() =>
                {
                    onOptionBPressed();

                    if (!reopeningPopup)
                    {
                        Close();
                    }
                });
            }

            if (onOptionCPressed != null)
            {
                optionCButton.onClick.AddListener(() =>
                {
                    onOptionCPressed();

                    if (!reopeningPopup)
                    {
                        Close();
                    }
                });
            }

            return this;
        }

        protected GrubbitPopupUI ResetButtonOptions()
        {
            optionAButton.onClick.RemoveAllListeners();
            optionBButton.onClick.RemoveAllListeners();
            optionCButton.onClick.RemoveAllListeners();
            optionAButton.gameObject.SetActive(true);
            optionBButton.gameObject.SetActive(false);
            optionCButton.gameObject.SetActive(false);
            optionAButton.onClick.AddListener(() => { Close(); });
            UpdateTextOptions("Ok");
            return this;
        }

        public GrubbitPopupUI UpdateTextOptions(string optionAButtonText, string optionBButtonText = "", string optionCButtonText = "")
        {
            try
            {
                if (optionAButton)
                {
                    optionAButton.GetComponentInChildren<TextMeshProUGUI>().text = optionAButtonText;
                }

                if (optionBButton && !string.IsNullOrEmpty(optionBButtonText))
                {
                    optionBButton.GetComponentInChildren<TextMeshProUGUI>().text = optionBButtonText;
                }

                if (optionCButton && !string.IsNullOrEmpty(optionCButtonText))
                {
                    optionCButton.GetComponentInChildren<TextMeshProUGUI>().text = optionCButtonText;
                }
            }
            catch (Exception e)
            {
                Utility.LogError(e, GetType());
            }

            return this;
        }
    }
}