using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region ReSharper Comments

// ReSharper disable Unity.InefficientPropertyAccess
// ReSharper disable FieldCanBeMadeReadOnly.Local

#endregion

namespace Grubbit
{
    public class GrubbitUIListItem : MonoBehaviour
    {
        [Header("General References")] public int id;
        public SmartButton interactButton;
        public bool expandWhenSelected;
        public float expandedWidth = 500f;

        public object itemData;

        [Header("Column References")] public Transform columnContainer;
        public TextMeshProUGUI textTemplate;

        [Header("Column References")] public Transform buttonsContainer;
        public SmartButton smartButtonTemplate;
        public bool attemptToGenerateMissingAssets = true;

        public List<ProColumnData> columns = new List<ProColumnData>();
        public List<ProColumnButtonData> buttons = new List<ProColumnButtonData>();
        [HideInInspector] public bool selected;
        [HideInInspector] public bool expanded;
        protected GrubbitUIList ParentGrubbitUiList;

        public Action<GrubbitUIListItem, bool> OnToggled;

        private float originalHeight = -1f;
        private LayoutElement baseLayoutElement = null;
        public LayoutElement BaseLayoutElement => baseLayoutElement != null ? baseLayoutElement : gameObject.GetOrAddComponent<LayoutElement>();
        private RectTransform baseRectTransform = null;
        public RectTransform BaseRectTransform => baseRectTransform != null ? baseRectTransform : gameObject.GetOrAddComponent<RectTransform>();

        /// <summary>
        /// Sets up the list item so it is ready for use. Only called once.
        /// </summary>
        public virtual void SetupElement(GrubbitUIList desiredParent)
        {
            if (ParentGrubbitUiList == null)
            {
                ParentGrubbitUiList = desiredParent;
            }

            if (textTemplate != null)
            {
                textTemplate.gameObject.SetActive(false);
            }

            if (smartButtonTemplate != null)
            {
                smartButtonTemplate.gameObject.SetActive(false);
            }

            if (interactButton != null)
            {
                interactButton.onClick.RemoveAllListeners();
                interactButton.onClick.AddListener(OnInteractButton_Pressed);

                interactButton.setInitialToggleState = false;
                interactButton.ToggleState(false, false);
            }
        }

        /// <summary>
        /// Resets the list item to a default state.
        /// </summary>
        public virtual void ClearElement()
        {
            for (var i = columns.Count - 1; i >= 0; --i)
            {
                RemoveColumn(columns[i], false);
            }

            for (var i = buttons.Count - 1; i >= 0; --i)
            {
                RemoveColumnButton(buttons[i], false);
            }

            selected = false;
            OnToggled = null;

            if (interactButton != null)
            {
                interactButton.ToggleState(false, false);
            }
        }

        /// <summary>
        /// A special function called that's meant to refresh the item, and is a light-weight alternative to a full recreation of the object.
        /// </summary>
        public virtual void RefreshElement()
        {
            // Nothing goes in here yet, since the base object doesn't need to do anything during a refresh
        }

        /// <summary>
        /// A special function that causes the list to grow vertically based on the newly given height.
        /// </summary>
        public virtual void ExpandElement(float newHeight)
        {
            if (expanded)
            {
                return;
            }

            if (BaseLayoutElement != null)
            {
                originalHeight = BaseLayoutElement.preferredHeight;
                BaseLayoutElement.preferredHeight = newHeight;
                expanded = true;
            }
            else if (BaseRectTransform != null)
            {
                originalHeight = BaseRectTransform.rect.height;
                BaseRectTransform.SetHeight(newHeight);
                expanded = true;
            }
        }

        /// <summary>
        /// A special function that causes the list to shrink vertically to the original height given.
        /// </summary>
        public virtual void ShrinkElement()
        {
            if (BaseLayoutElement != null)
            {
                BaseLayoutElement.preferredHeight = Mathf.Max(0f, originalHeight);
            }
            else if (BaseRectTransform != null)
            {
                BaseRectTransform.SetHeight(Mathf.Max(0f, originalHeight));
			}

			expanded = false;
        }

        public ProColumnData AddColumn(string text, Color textColor, float columnWidth = -1f, float columnHeight = -1f, int columnIndex = -1)
        {
            return AddColumn(BuildColumnData(text, textColor, columnWidth, columnHeight, columnIndex));
        }

        private static ProColumnData BuildColumnData(string text, Color textColor, float columnWidth, float columnHeight, int columnIndex = -1)
        {
            var newData = new ProColumnData
            {
                text = text,
                textColor = textColor,
                overrideColumnSize = columnWidth >= 0f || columnHeight >= 0f,
                columnWidth = columnWidth,
                columnHeight = columnHeight,
                overrideColumnIndex = columnIndex != -1,
                columnIndex = Mathf.Max(columnIndex, 0)
            };

            return newData;
        }

        public ProColumnData AddColumn(ProColumnData data)
        {
            if (columns.Contains(data))
            {
                Utility.LogWarning("This list item already contains the given column...", GetType());
                return null;
            }

            if (textTemplate != null)
            {
                data.textObject = Instantiate(textTemplate, columnContainer);
                data.textObject.gameObject.SetActive(true);
            }
            else
            {
                data.textObject = new GameObject().AddComponent<TextMeshProUGUI>();
                data.textObject.transform.SetParent(columnContainer);
                data.textObject.transform.localPosition = Vector3.zero;
            }

            data.textObject.name = "ColumnText";
            data.textObject.transform.SetAsLastSibling();
            data.textObject.text = data.text;
            data.textObject.color = data.textColor;

            if (data.overrideColumnSize)
            {
                var layoutElement = data.textObject.GetComponent<LayoutElement>();

                if (layoutElement == null)
                {
                    layoutElement = data.textObject.gameObject.AddComponent<LayoutElement>();
                }

                layoutElement.preferredWidth = data.columnWidth;
                layoutElement.preferredHeight = data.columnHeight;
            }

            if (data.overrideColumnIndex)
            {
                data.textObject.transform.SetSiblingIndex(data.columnIndex);
            }
            else
            {
                data.columnIndex = data.textObject.transform.GetSiblingIndex();
            }

            columns.Add(data);
            UpdateColumnIndices();
            return data;
        }

        public void RemoveColumn(string text)
        {
            for (var i = 0; i < columns.Count; ++i)
            {
                if (columns[i].text == text)
                {
                    RemoveColumn(columns[i]);
                    return;
                }
            }
        }

        public void RemoveColumn(int index)
        {
            if (columns.Count > index)
            {
                RemoveColumn(columns[index]);
            }
        }

        public void RemoveColumn(ProColumnData data, bool updateColumnIndices = true)
        {
            if (columns.Contains(data))
            {
                if (data.textObject != null)
                {
                    Destroy(data.textObject.gameObject);
                }

                columns.Remove(data);

                if (updateColumnIndices)
                {
                    UpdateColumnIndices();
                }
            }
            else
            {
                Utility.LogWarning("This list item does not contain the desired column, cannot remove...", GetType());
            }
        }

        private void UpdateColumnIndices()
        {
            for (var i = 0; i < columns.Count; ++i)
            {
                if (!columns[i].overrideColumnIndex && columns[i].textObject != null)
                {
                    columns[i].columnIndex = columns[i].textObject.transform.GetSiblingIndex();
                }
            }
        }

        public ProColumnButtonData AddColumnButton(Action<ProColumnButtonData> onButtonPressed, string text, Color textColor, Sprite icon, Color iconColor, Sprite fillIcon = null, Color fillColor = default(Color), bool toggleOnPress = false, bool startingState = false, float columnWidth = -1f, float columnHeight = -1f, int columnIndex = -1)
        {
            var newData = BuildColumnButtonData(onButtonPressed, icon, iconColor, fillIcon, fillColor, text, textColor, toggleOnPress, startingState, columnWidth, columnHeight, columnIndex);
            newData.parent = this;
            return AddColumnButton(newData);
        }

        public ProColumnButtonData AddColumnButton(Action<ProColumnButtonData> onButtonPressed, Sprite icon, Color iconColor, Sprite fillIcon = null, Color fillColor = default(Color), bool toggleOnPress = false, bool startingState = false, float columnWidth = -1f, float columnHeight = -1f, int columnIndex = -1)
        {
            var newData = BuildColumnButtonData(onButtonPressed, icon, iconColor, fillIcon, fillColor, "", Color.white, toggleOnPress, startingState, columnWidth, columnHeight, columnIndex);
            newData.parent = this;
            return AddColumnButton(newData);
        }

        public ProColumnButtonData AddColumnButton(Action<ProColumnButtonData> onButtonPressed, string text, Color textColor, bool toggleOnPress = false, bool startingState = false, float columnWidth = -1f, float columnHeight = -1f, int columnIndex = -1)
        {
            var newData = BuildColumnButtonData(onButtonPressed, null, Color.white, null, Color.white, text, textColor, toggleOnPress, startingState, columnWidth, columnHeight, columnIndex);
            newData.parent = this;
            return AddColumnButton(newData);
        }

        private static ProColumnButtonData BuildColumnButtonData(Action<ProColumnButtonData> onButtonPressed, Sprite icon, Color iconColor, Sprite fillIcon, Color fillColor, string text, Color textColor, bool toggleOnPress, bool startingState, float columnWidth, float columnHeight, int columnIndex)
        {
            var newData = new ProColumnButtonData
            {
                icon = icon,
                iconColor = iconColor,
                text = text,
                textColor = textColor,
                fillIcon = fillIcon,
                fillIconColor = fillColor,
                OnButtonPressed = onButtonPressed,
                toggleOnPress = toggleOnPress,
                startingState = startingState,
                overrideColumnSize = columnWidth >= 0f || columnHeight >= 0f,
                columnWidth = columnWidth,
                columnHeight = columnHeight,
                overrideColumnIndex = columnIndex != -1,
                columnIndex = Mathf.Max(columnIndex, 0)
            };

            return newData;
        }

        public ProColumnButtonData AddColumnButton(ProColumnButtonData data)
        {
            if (smartButtonTemplate == null)
            {
                Utility.LogError("There is no button template, cannot create a column button!", GetType());
                return null;
            }

            if (buttons.Contains(data))
            {
                Utility.LogWarning("This list item already contains the given column...", GetType());
                return null;
            }

            data.smartButtonObject = Instantiate(smartButtonTemplate, buttonsContainer);
            data.smartButtonObject.name = "ColumnButton";
            data.smartButtonObject.setInitialToggleState = true;
            data.smartButtonObject.initialToggleState = data.startingState;
            data.smartButtonObject.autoToggleOnPress = data.toggleOnPress;
            data.smartButtonObject.onClick.RemoveAllListeners();
            data.smartButtonObject.gameObject.SetActive(true);

            if (data.OnButtonPressed != null)
            {
                data.smartButtonObject.onClick.AddListener(() => { data.OnButtonPressed.Invoke(data); });
            }

            if (data.icon != null)
            {
                var imageObject = data.smartButtonObject.GetComponent<Image>();

                if (imageObject == null && attemptToGenerateMissingAssets)
                {
                    imageObject = new GameObject { name = "Image" }.AddComponent<Image>();
                    imageObject.transform.SetParent(data.smartButtonObject.transform);
                    imageObject.transform.position = Vector3.zero;
                    data.imageObject.raycastTarget = false;
                }

                if (imageObject != null)
                {
                    data.imageObject = imageObject;
                    data.imageObject.sprite = data.icon;
                    data.imageObject.color = data.iconColor;
                    data.imageObject.gameObject.SetActive(true);
                }

                if (data.fillIcon != null)
                {
                    var fillImageObjects = data.imageObject.GetComponentsInChildren<Image>();
                    Image fillImageObject = null;

                    // We need to do this because GetComponentInChildren will return the parent object's component if it exists, and we don't
                    // want that in this instance
                    if (fillImageObjects.Length > 1)
                    {
                        fillImageObject = fillImageObjects[1];
                    }

                    if (fillImageObject == null && attemptToGenerateMissingAssets)
                    {
                        fillImageObject = new GameObject { name = "FillImage" }.AddComponent<Image>();
                        fillImageObject.transform.SetParent(data.smartButtonObject.transform);
                        fillImageObject.transform.position = Vector3.zero;
                    }

                    if (fillImageObject != null)
                    {
                        data.fillImageObject = fillImageObject;
                        data.fillImageObject.sprite = data.fillIcon;
                        data.fillImageObject.color = data.fillIconColor;
                        data.fillImageObject.type = Image.Type.Filled;
                        data.fillImageObject.fillAmount = 0f;
                        data.fillImageObject.raycastTarget = false;
                        data.fillImageObject.gameObject.SetActive(true);
                    }
                }
            }

            var textObject = data.smartButtonObject.GetComponentInChildren<TextMeshProUGUI>();

            if (textObject == null && !string.IsNullOrEmpty(data.text))
            {
                textObject = new GameObject { name = "Text" }.AddComponent<TextMeshProUGUI>();
                textObject.transform.SetParent(data.smartButtonObject.transform);
                textObject.transform.position = Vector3.zero;
            }

            if (textObject != null)
            {
                data.textObject = textObject;
                data.textObject.text = data.text ?? "";
                data.textObject.color = data.textColor;
                data.textObject.raycastTarget = false;
                data.textObject.gameObject.SetActive(!string.IsNullOrEmpty(data.text));
            }

            if (data.overrideColumnSize)
            {
                var layoutElement = data.smartButtonObject.GetComponent<LayoutElement>();

                if (layoutElement == null)
                {
                    layoutElement = data.smartButtonObject.gameObject.AddComponent<LayoutElement>();
                }

                layoutElement.preferredWidth = data.columnWidth;
                layoutElement.preferredHeight = data.columnHeight;

                if (data.icon != null)
                {
                    var imageRect = data.imageObject.GetComponent<RectTransform>();
                    imageRect.SetHeight(data.columnHeight);
                    imageRect.SetWidth(data.columnWidth);
                    imageRect.anchoredPosition = Vector2.zero;
                    var centeredV2 = new Vector2(0.5f, 0.5f);
                    imageRect.anchorMin = centeredV2;
                    imageRect.anchorMax = centeredV2;
                    imageRect.pivot = centeredV2;
                }
                else if (!string.IsNullOrEmpty(data.text))
                {
                    var textRect = data.textObject.GetComponent<RectTransform>();
                    textRect.SetHeight(data.columnHeight);
                    textRect.SetWidth(data.columnWidth);
                    textRect.anchoredPosition = Vector2.zero;
                    var centeredV2 = new Vector2(0.5f, 0.5f);
                    textRect.anchorMin = centeredV2;
                    textRect.anchorMax = centeredV2;
                    textRect.pivot = centeredV2;
                }
            }

            if (data.overrideColumnIndex)
            {
                data.smartButtonObject.transform.SetSiblingIndex(data.columnIndex);
            }
            else
            {
                data.columnIndex = data.smartButtonObject.transform.GetSiblingIndex();
            }

            buttons.Add(data);
            UpdateColumnIndices();
            return data;
        }

        public void RemoveColumnButton(string text)
        {
            for (var i = 0; i < buttons.Count; ++i)
            {
                if (buttons[i].text == text)
                {
                    RemoveColumnButton(buttons[i]);
                    return;
                }
            }
        }

        public void RemoveColumnButton(ProColumnButtonData data, bool updateColumnButtonIndices = true)
        {
            if (buttons.Contains(data))
            {
                if (data.smartButtonObject != null)
                {
                    Destroy(data.smartButtonObject.gameObject);
                }

                if (data.textObject != null)
                {
                    Destroy(data.textObject.gameObject);
                }

                buttons.Remove(data);

                if (updateColumnButtonIndices)
                {
                    UpdateColumnButtonIndices();
                }
            }
            else
            {
                Utility.LogWarning("This list item does not contain the desired column, cannot remove...", GetType());
            }
        }

        private void UpdateColumnButtonIndices()
        {
            for (var i = 0; i < buttons.Count; ++i)
            {
                if (!buttons[i].overrideColumnIndex && buttons[i].textObject != null)
                {
                    buttons[i].columnIndex = buttons[i].textObject.transform.GetSiblingIndex();
                }
            }
        }

        private void OnInteractButton_Pressed()
        {
            // We do this inside of an OnClick event to disregard any weirdness that
            // may happen when the button's state is being toggled
            var newState = !interactButton.CurrentlyEnabled;
            OnToggled?.Invoke(this, newState);
        }

        public void SetSelectionState(bool desiredState)
        {
            OnToggled?.Invoke(this, desiredState);
        }

        public void Select()
        {
            ChangeSelectedState(true);
        }

        public void Deselect()
        {
            ChangeSelectedState(false);
        }

        private void ChangeSelectedState(bool isSelected)
        {
            selected = isSelected;

            if (expandWhenSelected)
            {
                if (selected)
                {
                    ExpandElement(expandedWidth);
                }
                else
                {
                    ShrinkElement();
                }
            }

            if (interactButton)
            {
                interactButton.ToggleState(isSelected, false);
            }
        }
    }
}