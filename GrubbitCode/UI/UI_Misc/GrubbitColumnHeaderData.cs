using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Grubbit
{
	[Serializable]
	public class ProColumnHeaderData
	{
		public GrubbitEnums.ColumnHeaderType type;

		public string titleText;
		public Color titleTextColor = Color.white;

		public string subtitleText;
		public Color subtitleTextColor = Color.white;

		public bool overrideColumnSize;
		public float columnWidth = 100f;
		public float columnHeight = 50f;

		public bool overrideColumnIndex;
		public int columnIndex;

		public bool canSort;
		public bool currentlySelected;
		public GrubbitEnums.SortOrder currentSortOrder;
		public GrubbitEnums.SortType currentSortType;
		public Sprite ascendingSortSprite;
		public Sprite descendingSortSprite;
		public Color sortSpriteColor;

		public SmartButton interactSmartButton;
		public TextMeshProUGUI titleTextTMP;
		public TextMeshProUGUI subtitleTextTMP;
		public Image sortImage;

		public bool adjustTitleTextPos;
		public bool adjustSubtitleTextPos;
		public bool adjustSortImagePos;
		public float subtitleHeight = 25f;

		private TextMeshProUGUI titleTextTemplate;
		private TextMeshProUGUI subtitleTextTemplate;
		private SmartButton smartButtonTemplate;
		private GrubbitUIList parentGrubbitUiList;

		public static ProColumnHeaderData Generate(string desiredText, Color desiredColor, GrubbitEnums.ColumnHeaderType desiredType, bool adjustTextPos = false)
		{
			var data = new ProColumnHeaderData()
			{
				titleText = desiredText,
				titleTextColor = desiredColor,
				adjustTitleTextPos = adjustTextPos,
				type = desiredType
			};

			return data;
		}

		public ProColumnHeaderData AddSubtitleText(string desiredSubtitleText, Color desiredSubtitleColor, bool adjustPos = false)
		{
			subtitleText = desiredSubtitleText;
			subtitleTextColor = desiredSubtitleColor;
			adjustSubtitleTextPos = adjustPos;
			return this;
		}

		public ProColumnHeaderData SetSortType(GrubbitEnums.SortType desiredSortType, bool adjustPos = false)
		{
			if (type == GrubbitEnums.ColumnHeaderType.TextOnly)
			{
				return this;
			}

			canSort = true;
			currentSortType = desiredSortType;
			currentSortOrder = GrubbitEnums.SortOrder.Ascending;
			return this;
		}

		public ProColumnHeaderData SetCustomSize(float desiredColumnWidth, float desiredColumnHeight, float desiredSubtitleHeight = -1f)
		{
			overrideColumnSize = true;
			columnWidth = desiredColumnWidth > 0 ? desiredColumnWidth : columnWidth;
			columnHeight = desiredColumnHeight > 0 ? desiredColumnHeight : columnHeight;
			subtitleHeight = desiredSubtitleHeight > 0 ? desiredSubtitleHeight : subtitleHeight;
			return this;
		}

		public ProColumnHeaderData SetCustomIndex(int desiredColumnIndex)
		{
			overrideColumnIndex = true;
			columnIndex = desiredColumnIndex;
			return this;
		}

		public ProColumnHeaderData AddSortImageTemplate(Sprite desiredAscendingSprite, Sprite desiredDescendingSprite, Color desiredColor)
		{
			ascendingSortSprite = desiredAscendingSprite;
			descendingSortSprite = desiredDescendingSprite;
			sortSpriteColor = desiredColor;
			return this;
		}

		public ProColumnHeaderData AddTitleTextTemplate(TextMeshProUGUI desiredTitleTextTemplate)
		{
			titleTextTemplate = desiredTitleTextTemplate;
			return this;
		}

		public ProColumnHeaderData AddSubtitleTextTemplate(TextMeshProUGUI desiredSubtitleTextTemplate)
		{
			subtitleTextTemplate = desiredSubtitleTextTemplate;
			adjustSubtitleTextPos = false;
			return this;
		}

		public ProColumnHeaderData AddSmartButtonTemplate(SmartButton desiredSmartButtonTemplate)
		{
			smartButtonTemplate = desiredSmartButtonTemplate;
			return this;
		}

		public ProColumnHeaderData Construct(Transform desiredColumnContainer, GrubbitUIList desiredParentGrubbitUiList)
		{
			if (string.IsNullOrEmpty(titleText))
			{
				Utility.LogError("There is no title text for this column header, there always needs to be a title text!", GetType());
				titleText = "NULL";
			}

			if (desiredParentGrubbitUiList != null)
			{
				parentGrubbitUiList = desiredParentGrubbitUiList;
			}

			switch (type)
			{
				case GrubbitEnums.ColumnHeaderType.Interactable:
					// Generate the smart button
					interactSmartButton = smartButtonTemplate != null ? Object.Instantiate(smartButtonTemplate, desiredColumnContainer) : new GameObject().AddComponent<SmartButton>();
					interactSmartButton.gameObject.SetActive(true);
					interactSmartButton.name = "ColumnButton";
					interactSmartButton.transform.SetAsLastSibling();
					interactSmartButton.onClick.RemoveAllListeners();

					// Try to find the first child object to the button and set that as the title text, if that doesn't exit then create a new text object
					var childTextObjects = interactSmartButton.GetComponentsInChildren<TextMeshProUGUI>(true);
					titleTextTMP = childTextObjects.Length > 0 ? childTextObjects[0] : Utility.CreateTMPObject(interactSmartButton.transform);

					// Let's just never let the text be a component of the main button for simplicity sake
					if (titleTextTMP.gameObject == interactSmartButton.gameObject)
					{
						Object.Destroy(titleTextTMP);
						titleTextTMP = Utility.CreateTMPObject(interactSmartButton.transform);
					}

					// Set up the title text
					if (titleTextTMP != null)
					{
						titleTextTMP.gameObject.name = "ButtonTitleText";
						titleTextTMP.gameObject.SetActive(true);
						titleTextTMP.raycastTarget = false;

						if (adjustTitleTextPos)
						{
							titleTextTMP.transform.localPosition = Vector3.zero;
							var titleRect = titleTextTMP.rectTransform;
							titleRect.anchoredPosition = Vector3.zero;
						}

						titleTextTMP.text = titleText;
						titleTextTMP.color = titleTextColor;
					}

					// If there should be a subtitle text, add it
					if (!string.IsNullOrEmpty(subtitleText))
					{
						subtitleTextTMP = childTextObjects.Length > 1 ? childTextObjects[1] : Utility.CreateTMPObject(interactSmartButton.transform);

						if (subtitleTextTMP != null)
						{
							subtitleTextTMP.gameObject.name = "ButtonSubtitleText";
							subtitleTextTMP.gameObject.SetActive(true);
							subtitleTextTMP.raycastTarget = false;

							if (adjustSubtitleTextPos)
							{
								subtitleTextTMP.transform.localPosition = Vector3.zero;
								var subtitleRect = subtitleTextTMP.rectTransform;
								subtitleRect.anchoredPosition = Vector3.zero;
							}

							subtitleTextTMP.text = subtitleText;
							subtitleTextTMP.color = subtitleTextColor;
						}
					}

					// If there should be a sort image, add it
					if (canSort)
					{
						if (titleTextTMP != null)
						{
							var imageChild = titleTextTMP.GetComponentInChildren<Image>(true);

							if (imageChild != null && imageChild.transform != titleTextTMP.transform)
							{
								sortImage = imageChild;
							}
						}
						else
						{
							sortImage = Utility.CreateImageObject(interactSmartButton.transform);
						}

						if (sortImage != null)
						{
							sortImage.gameObject.name = "SortImage";
							sortImage.gameObject.SetActive(false);
							sortImage.raycastTarget = false;
							sortImage.color = sortSpriteColor;

							if (adjustSortImagePos)
							{
								sortImage.transform.localPosition = Vector3.zero;
								var sortImageRect = sortImage.rectTransform;
								sortImageRect.anchoredPosition = Vector3.zero;
							}

							if (ascendingSortSprite != null)
							{
								sortImage.sprite = ascendingSortSprite;
							}

							interactSmartButton.onClick.AddListener(() =>
							{
								if (currentlySelected)
								{
									currentSortOrder = currentSortOrder == GrubbitEnums.SortOrder.Ascending ? GrubbitEnums.SortOrder.Descending : GrubbitEnums.SortOrder.Ascending;
									sortImage.sprite = currentSortOrder == GrubbitEnums.SortOrder.Ascending ? ascendingSortSprite : descendingSortSprite;
									parentGrubbitUiList.Sort();
								}
								else
								{
									if (parentGrubbitUiList != null)
									{
										parentGrubbitUiList.SetActiveColumnHeader(this);
										parentGrubbitUiList.Sort();
									}
								}
							});
						}
					}

					if (overrideColumnIndex)
					{
						interactSmartButton.transform.SetSiblingIndex(columnIndex);
					}
					else
					{
						columnIndex = interactSmartButton.transform.GetSiblingIndex();
					}

					break;
				case GrubbitEnums.ColumnHeaderType.TextOnly:
					titleTextTMP = titleTextTemplate != null ? Object.Instantiate(titleTextTemplate, desiredColumnContainer) : Utility.CreateTMPObject(desiredColumnContainer);

					if (titleTextTMP != null)
					{
						titleTextTMP.name = "ColumnText";
						titleTextTMP.raycastTarget = false;
						titleTextTMP.gameObject.SetActive(true);
						titleTextTMP.text = titleText;
						titleTextTMP.color = titleTextColor;
					}

					if (!string.IsNullOrEmpty(subtitleText))
					{
						subtitleTextTMP = subtitleTextTemplate != null ? Object.Instantiate(subtitleTextTemplate, desiredColumnContainer) : Utility.CreateTMPObject(desiredColumnContainer);

						if (subtitleTextTMP != null)
						{
							subtitleTextTMP.gameObject.name = "ButtonSubtitleText";
							subtitleTextTMP.raycastTarget = false;
							subtitleTextTMP.gameObject.SetActive(true);

							if (adjustSubtitleTextPos)
							{
								subtitleTextTMP.transform.localPosition = Vector3.zero;
								var subtitleRect = subtitleTextTMP.rectTransform;
								subtitleRect.anchoredPosition = Vector3.zero;
							}

							subtitleTextTMP.text = subtitleText;
							subtitleTextTMP.color = subtitleTextColor;
						}
					}

					if (overrideColumnIndex)
					{
						titleTextTMP.transform.SetSiblingIndex(columnIndex);
					}
					else
					{
						columnIndex = titleTextTMP.transform.GetSiblingIndex();
					}

					break;
			}

			if (overrideColumnSize)
			{
				if (interactSmartButton != null)
				{
					var buttonLE = interactSmartButton.gameObject.GetOrAddComponent<LayoutElement>();
					buttonLE.preferredWidth = columnWidth;
					buttonLE.preferredHeight = columnHeight;
				}

				if (titleTextTMP != null && !canSort)
				{
					var titleTextLE = titleTextTMP.gameObject.GetOrAddComponent<LayoutElement>();
					titleTextLE.preferredWidth = columnWidth;
					titleTextLE.preferredHeight = columnHeight;
				}

				if (subtitleTextTMP != null && !canSort)
				{
					var subtitleTextLE = subtitleTextTMP.gameObject.GetOrAddComponent<LayoutElement>();
					subtitleTextLE.preferredWidth = columnWidth;
					subtitleTextLE.preferredHeight = subtitleHeight;
				}
			}

			smartButtonTemplate = null;
			titleTextTemplate = null;
			subtitleTextTemplate = null;
			return this;
		}
	}
}