using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [DisallowMultipleComponent]
    public class DataListItemBinder : MonoBehaviour
    {
        [SerializeField]
        private List<BaseLocalViewModel> localViewModelList = null;

        private int boundSourceId;
        private int boundListId;
        private int boundIndex;
        private DataList boundDataList;

        private void Awake()
        {
            foreach (var item in localViewModelList)
            {
                if (item is ISelectableViewModel selectable)
                {
                    selectable.SelectionChanged += (isSelected, index) => OnViewModelSelectionChanged(item, isSelected, index);
                }
            }
        }

        public void SetData(int sourceId, int listId, int index)
        {
            boundSourceId = sourceId;
            boundListId = listId;
            boundIndex = index;
            // TODO: Implement a way to resolve DataList from listId
            boundDataList = null; // Replace with actual lookup, e.g. DataSourceManager.GetDataListById(listId)
            foreach (var item in localViewModelList)
            {
                item.SetBindingData(sourceId, listId, index);
                if (item is ISelectableViewModel selectable)
                {
                    selectable.Index = index;
                }
            }
        }

        private void OnViewModelSelectionChanged(BaseLocalViewModel viewModel, bool isSelected, int index)
        {
            if (isSelected && boundDataList != null)
            {
                boundDataList.SelectedIndex = index;
            }
            else if (boundDataList.SelectedIndex == index && !isSelected)
            {
                boundDataList.SelectedIndex = -1; // Deselect if the selected index matches the one being deselected
            }
        }

        public void ClearData()
        {
            foreach (var item in localViewModelList)
            {
                item.ClearBindingData();
            }
        }
    }
}