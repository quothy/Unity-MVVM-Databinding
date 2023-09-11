using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class ViewModelDataSource : BaseDataSource
    {
        private Dictionary<string, int> itemNameToIdLookup = null;

        private BaseViewModel viewModel = null;

        /// <summary>
        /// Use this call to initialize at runtime
        /// </summary>
        /// <param name="viewModel"></param>
        internal void InitializeFromViewModel(BaseViewModel viewModel)
        {
            this.viewModel = viewModel;
            bool isIdModifiedAtRuntime = !viewModel.IsGlobalSource;
            string viewModelName = viewModel.GetType().ToString();
            if (isIdModifiedAtRuntime)
            {
                viewModelName = ResolveNameWithRuntimeId(viewModelName, viewModel.gameObject.GetInstanceID());
            }

            Initialize(viewModelName, isIdModifiedAtRuntime);
        }

        public void LoadDataItems(List<IDataItem> dataItems)
        {
            itemNameToIdLookup = new Dictionary<string, int>(dataItems.Count);
            foreach (IDataItem item in dataItems)
            {
                item.RuntimeInit(viewModel);
                AddItem(item);
                itemNameToIdLookup[item.Name] = item.Id;
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            if (itemNameToIdLookup.TryGetValue(propertyName, out int id))
            {
                OnItemChangedInSource(id);
            }
        }
    }
}