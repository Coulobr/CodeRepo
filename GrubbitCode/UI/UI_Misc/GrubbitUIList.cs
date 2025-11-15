using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grubbit
{
    public class GrubbitUIList : MonoBehaviour
    {
        [Header("General References")] public GrubbitUIListItem GrubbitUiListItemTemplate;
        public Transform usedContainer;
        public Transform unusedContainer;
        public bool manuallyActivateGameObjects;
        public string desiredObjectName = "GrubbitUIListItem";

        [Header("Settings")] public int numOfObjectsToMake = 10;
        public bool canExpandCount;
        public bool setAsFirstSibling;
        public bool canSelect = true;
        public bool allowMultipleSelections;
        public bool newSelectionOverridesOld;
        public float columnWidth = 175f;
        public float columnHeight = 75f;

        [Header("Asynchronous Settings")] public bool asynchronous;
        public GameObject refreshGO;
        public int batchCount = 9;

        [HideInInspector] public List<GrubbitUIListItem> selectedItems = new List<GrubbitUIListItem>();
        [HideInInspector] public List<GrubbitUIListItem> allProListItems = new List<GrubbitUIListItem>();
        [HideInInspector] public List<GrubbitUIListItem> usedProListItems = new List<GrubbitUIListItem>();
        [HideInInspector] public List<ProColumnHeaderData> columnHeaders = new List<ProColumnHeaderData>();
        [HideInInspector] public ProColumnHeaderData currentActiveColumn;
        public Action<GrubbitUIListItem, bool, List<GrubbitUIListItem>> OnProListItemToggled;
        protected Action OnBuildListCompleted;
        protected IEnumerator buildListCR;
        protected bool generatedAssets;

        protected virtual void Start()
        {
            if (!generatedAssets)
            {
                GenerateAssets();
            }
        }

        /// <summary>
        /// Sets up the list so it is ready for use. Only called once.
        /// </summary>
        public virtual void SetupElement()
        {
            if (!generatedAssets)
            {
                GenerateAssets();
            }
        }

        /// <summary>
        /// Resets the list to a default state, deactivating all of the currently active list items in the process.
        /// </summary>
        public virtual void ResetElement()
        {
            if (manuallyActivateGameObjects)
            {
                DisableAllGameObjects();
            }

            for (var i = usedProListItems.Count - 1; i >= 0; --i)
            {
                if (usedProListItems[i].expanded)
                {
                    usedProListItems[i].ShrinkElement();
                }

                DeactivateProListItem(usedProListItems[i]);
            }

            selectedItems.Clear();
            OnBuildListCompleted = null;
        }

        /// <summary>
        /// A special function called that's meant to refresh the currently selected list of items.
        /// </summary>
        public virtual void RefreshElement()
        {
            for (var i = usedProListItems.Count - 1; i >= 0; --i)
            {
                usedProListItems[i].RefreshElement();
            }
        }

        protected T ActivateNextFreeProListItemAsType<T>() where T : GrubbitUIListItem
        {
            return ActivateNextFreeProListItem() as T;
        }

        protected GrubbitUIListItem ActivateNextFreeProListItem()
        {
            return ActivateProListItem(GetFreeProListItem());
        }

        protected T GetFreeProListItemAsType<T>() where T : GrubbitUIListItem
        {
            return GetFreeProListItem() as T;
        }

        protected GrubbitUIListItem GetFreeProListItem()
        {
            var freeProListItems = allProListItems.Except(usedProListItems).ToList();

            if (freeProListItems.Count == 0)
            {
                if (canExpandCount)
                {
                    freeProListItems.Add(GenerateSingleAsset(allProListItems.Count));
                }
                else
                {
                    return null;
                }
            }

            return freeProListItems[0];
        }

        protected List<T> GetUsedProListItemsAsType<T>() where T : GrubbitUIListItem
        {
            return usedProListItems.Cast<T>().ToList();
        }
        
        protected List<T> GetAllProListItemsAsType<T>() where T : GrubbitUIListItem
        {
            return allProListItems.Cast<T>().ToList();
        }

        public void ActivateAllGameObjects()
        {
            for (var i = 0; i < usedProListItems.Count; ++i)
            {
                usedProListItems[i].gameObject.SetActive(true);
            }
        }

        public void DisableAllGameObjects()
        {
            for (var i = 0; i < usedProListItems.Count; ++i)
            {
                usedProListItems[i].gameObject.SetActive(false);
            }
        }

        protected T ActivateProListItemAsType<T>(GrubbitUIListItem grubbitUiListItem) where T : GrubbitUIListItem
        {
            return ActivateProListItem(grubbitUiListItem) as T;
        }

        protected T ActivateProListItemAsType<T>(T desiredProListItem) where T : GrubbitUIListItem
        {
            return ActivateProListItem(desiredProListItem) as T;
        }

        protected GrubbitUIListItem ActivateProListItem(GrubbitUIListItem desiredGrubbitUiListItem)
        {
            if (!manuallyActivateGameObjects)
            {
                desiredGrubbitUiListItem.gameObject.SetActive(true);
            }

            desiredGrubbitUiListItem.transform.SetParent(usedContainer);

            if (setAsFirstSibling)
            {
                desiredGrubbitUiListItem.transform.SetAsFirstSibling();
            }
            else
            {
                desiredGrubbitUiListItem.transform.SetAsLastSibling();
            }

            usedProListItems.Add(desiredGrubbitUiListItem);
            return desiredGrubbitUiListItem;
        }

        protected void DeactivateProListItem(GrubbitUIListItem desiredGrubbitUiListItem)
        {
            if (!manuallyActivateGameObjects)
            {
                desiredGrubbitUiListItem.gameObject.SetActive(false);
            }

            desiredGrubbitUiListItem.transform.SetParent(unusedContainer);
            desiredGrubbitUiListItem.ClearElement();

            if (usedProListItems.Contains(desiredGrubbitUiListItem))
            {
                usedProListItems.Remove(desiredGrubbitUiListItem);
            }
        }

        protected T GetProListItemByIDAsType<T>(int id) where T : GrubbitUIListItem
        {
            return GetProListItemByID(id) as T;
        }

        protected GrubbitUIListItem GetProListItemByID(int id)
        {
            return allProListItems.FirstOrDefault(t => t.id == id);
        }

        protected GrubbitUIListItem GenerateSingleAsset(int num)
        {
            var newProListItem = Instantiate(GrubbitUiListItemTemplate, unusedContainer);
            newProListItem.name = $"{desiredObjectName}_{num}";
            newProListItem.id = num;
            newProListItem.SetupElement(this);
            newProListItem.ClearElement();
            allProListItems.Add(newProListItem);
            newProListItem.gameObject.SetActive(false);
            return newProListItem;
        }

        protected void GenerateAssets()
        {
            for (var i = 0; i < numOfObjectsToMake - 1; ++i)
            {
                GenerateSingleAsset(i + 1);
            }

            GrubbitUiListItemTemplate.name = $"{desiredObjectName}_{numOfObjectsToMake}";
            GrubbitUiListItemTemplate.id = numOfObjectsToMake;
            GrubbitUiListItemTemplate.transform.SetParent(unusedContainer);
            GrubbitUiListItemTemplate.SetupElement(this);
            GrubbitUiListItemTemplate.ClearElement();
            allProListItems.Add(GrubbitUiListItemTemplate);
            GrubbitUiListItemTemplate.gameObject.SetActive(false);
            generatedAssets = true;
        }

        public virtual void BuildList(Action onCompleted = null)
        {
            ResetElement();

            OnBuildListCompleted += onCompleted;

            if (asynchronous)
            {
                if (refreshGO)
                {
                    refreshGO.SetActive(true);
                }

                if (buildListCR == null)
                {
                    buildListCR = BuildListCR();
                    StartCoroutine(buildListCR);
                }
            }
            else
            {
                BuildListInstant();
            }
        }

        protected virtual void BuildListInstant()
        {
            if (currentActiveColumn == null && columnHeaders.Count > 0)
            {
                SetActiveColumnHeader(columnHeaders[0]);
            }

            Sort();
            OnBuildListCompleted?.Invoke();
            OnBuildListCompleted = null;
        }

        protected virtual IEnumerator BuildListCR()
        {
            yield return null;

            if (currentActiveColumn == null && columnHeaders.Count > 0)
            {
                SetActiveColumnHeader(columnHeaders[0]);
            }

            Sort();
            OnBuildListCompleted?.Invoke();
            OnBuildListCompleted = null;
        }

        public void RemoveColumnHeader(string text)
        {
            for (var i = 0; i < columnHeaders.Count; ++i)
            {
                if (columnHeaders[i].titleText == text)
                {
                    RemoveColumnHeader(columnHeaders[i]);
                }
            }
        }

        public void AddColumnHeader(ProColumnHeaderData data)
        {
            if (data != null)
            {
                columnHeaders.Add(data);
            }
        }

        public void RemoveColumnHeader(int index)
        {
            if (columnHeaders.Count > index)
            {
                RemoveColumnHeader(columnHeaders[index]);
            }
        }

        public void RemoveColumnHeader(ProColumnHeaderData data)
        {
            if (columnHeaders.Contains(data))
            {
                if (data.subtitleTextTMP != null)
                {
                    Destroy(data.subtitleTextTMP.gameObject);
                }

                if (data.titleTextTMP != null)
                {
                    Destroy(data.titleTextTMP.gameObject);
                }

                if (data.interactSmartButton != null)
                {
                    Destroy(data.interactSmartButton.gameObject);
                }

                columnHeaders.Remove(data);
            }
            else
            {
                Utility.LogWarning("This list header does not contain the desired column, cannot remove...", GetType());
            }
        }

        public void RemoveAllColumnHeaders()
        {
            for (var i = columnHeaders.Count - 1; i >= 0; --i)
            {
                RemoveColumnHeader(columnHeaders[i]);
            }
        }

        public void SetActiveColumnHeader(ProColumnHeaderData desiredColumnHeaderData)
        {
            if (desiredColumnHeaderData.canSort)
            {
                // Deactivate old column
                if (currentActiveColumn.sortImage)
                {
                    currentActiveColumn.sortImage.gameObject.SetActive(false);
                }

                currentActiveColumn.currentlySelected = false;

                // Activate new column
                currentActiveColumn = desiredColumnHeaderData;
                currentActiveColumn.currentlySelected = true;
                currentActiveColumn.currentSortOrder = GrubbitEnums.SortOrder.Ascending;

                if (currentActiveColumn.sortImage)
                {
                    currentActiveColumn.sortImage.gameObject.SetActive(true);
                    currentActiveColumn.sortImage.sprite = currentActiveColumn.ascendingSortSprite;
                }
            }
        }

        public void Sort()
        {
            if (currentActiveColumn == null)
            {
                return;
            }

            var itemsThatCanSort = new List<GrubbitUIListItem>();

            // This is to check to see if a column does not exist for a pro list item, if the desired column does not exist then just put the item at the bottom
            // We also need to subtract 1 from the column index because the template item is hidden, but is still a child
            var desiredIndex = Mathf.Max(0, currentActiveColumn.columnIndex - 1);

            itemsThatCanSort.AddRange(usedProListItems.Where(item => item.columns != null
                                                                     && item.columns.Count > desiredIndex
                                                                     && item.columns[desiredIndex] != null
                                                                     && !string.IsNullOrEmpty(item.columns[desiredIndex].text)
                                                                     && !item.columns[desiredIndex].text.ToLower().Contains("n/a")).ToList());

            var itemsToPutAtBottom = usedProListItems.Except(itemsThatCanSort).ToList();
            var sortedItems = new List<GrubbitUIListItem>();

            switch (currentActiveColumn.currentSortType)
            {
                default:
                case GrubbitEnums.SortType.Alphabetical:
                    sortedItems.AddRange(currentActiveColumn.currentSortOrder == GrubbitEnums.SortOrder.Ascending ? itemsThatCanSort.OrderBy(item => item.columns[desiredIndex].text).ToList() : itemsThatCanSort.OrderByDescending(item => item.columns[desiredIndex].text).ToList());
                    break;
                case GrubbitEnums.SortType.IntValue:
                    sortedItems.AddRange(currentActiveColumn.currentSortOrder == GrubbitEnums.SortOrder.Ascending ? itemsThatCanSort.OrderByDescending(item => Convert.ToInt32(item.columns[desiredIndex].text.Replace("°", ""))).ToList() : itemsThatCanSort.OrderBy(item => Convert.ToInt32(item.columns[desiredIndex].text.Replace("°", ""))).ToList());
                    break;
                case GrubbitEnums.SortType.FloatValue:
                    sortedItems.AddRange(currentActiveColumn.currentSortOrder == GrubbitEnums.SortOrder.Ascending ? itemsThatCanSort.OrderByDescending(item => Convert.ToSingle(item.columns[desiredIndex].text.Replace("°", ""))).ToList() : itemsThatCanSort.OrderBy(item => Convert.ToSingle(item.columns[desiredIndex].text.Replace("°", ""))).ToList());
                    break;
                case GrubbitEnums.SortType.Date:
                    sortedItems.AddRange(currentActiveColumn.currentSortOrder == GrubbitEnums.SortOrder.Ascending ? itemsThatCanSort.OrderByDescending(item => DateTime.Parse(item.columns[desiredIndex].text)).ToList() : itemsThatCanSort.OrderBy(item => DateTime.Parse(item.columns[desiredIndex].text)).ToList());
                    break;
            }

            sortedItems.AddRange(itemsToPutAtBottom);

            for (var i = 0; i < sortedItems.Count; ++i)
            {
                sortedItems[i].transform.SetSiblingIndex(i);
            }
        }

        public void DeselectAllItems()
        {
            for (var i = selectedItems.Count - 1; i >= 0; --i)
            {
                selectedItems[i].SetSelectionState(false);
            }
        }

        public void DeselectItem(object desiredData)
        {
            foreach (var item in selectedItems.Where(item => item.itemData != null && item.itemData.Equals(desiredData)))
            {
                item.SetSelectionState(false);
                return;
            }
        }

        public void SelectItem(object desiredData)
        {
	        foreach (var item in selectedItems.Where(item => item.itemData != null && item.itemData.Equals(desiredData)))
	        {
		        item.SetSelectionState(true);
		        return;
	        }
        }

        public void PruneSelectedItems()
        {
            for (var i = selectedItems.Count - 1; i >= 0; i--)
            {
                if (!selectedItems[i].gameObject.activeSelf)
                {
                    selectedItems[i].Deselect();
                    selectedItems.RemoveAt(i);
                }
            }
        }

        protected void OnItemToggled(GrubbitUIListItem item, bool newState)
        {
            if (!canSelect)
            {
                return;
            }

            if (item != null)
            {
                if (newState)
                {
                    if (!allowMultipleSelections && selectedItems.Count > 0)
                    {
                        if (newSelectionOverridesOld)
                        {
                            DeselectAllItems();
                        }
                        else
                        {
                            item.Deselect();

                            if (selectedItems.Contains(item))
                            {
                                selectedItems.Remove(item);
                            }

                            return;
                        }
                    }

                    if (!selectedItems.Contains(item))
                    {
                        item.Select();
                        selectedItems.Add(item);
                    }
                }
                else
                {
                    if (selectedItems.Contains(item))
                    {
                        item.Deselect();
                        selectedItems.Remove(item);
                    }
                }
            }

            OnProListItemToggled?.Invoke(item, newState, selectedItems);
        }
    }
}