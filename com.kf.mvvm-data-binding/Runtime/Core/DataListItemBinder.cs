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

        public void SetData(int sourceId, int listId, int index)
        {
            foreach (var item in localViewModelList)
            {
                item.SetBindingData(sourceId, listId, index);
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