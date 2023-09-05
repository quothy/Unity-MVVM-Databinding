using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public enum DataSourceIdResolutionType
    {
        None,
        ManuallySetInstance,
        GetComponentInParent,
    }

    [Serializable]
    public abstract class BaseBinder<T> : IBinder
    {
        [SerializeField]
        private DataRecord dataRecord = null;

        [ConditionalVisibility(nameof(editor_RecordRequiresExtraData), ConditionResultType.ShowIfEquals)]
        [SerializeField]
        private DataSourceIdResolutionType resolutionType = DataSourceIdResolutionType.None;

        [ConditionalVisibility(nameof(editor_SourceInstanceNeedsSet), ConditionResultType.ShowIfEquals)]
        [SerializeField]
        private GameObject dataSourceInstance = null;

        [ConditionalVisibility(nameof(DataRecordValid), ConditionResultType.ShowIfEquals)]
        [DropdownSelection(nameof(ItemNameOptions), nameof(SelectedItemName))]
        [SerializeField]
        protected int itemId = -1;

        public abstract string Name { get; }

        public bool DataRecordValid
        {
            get => dataRecord != null;
        }

        protected string SelectedItemName 
        {
            get
            {
                string selected = string.Empty;
                if (dataRecord != null)
                {
                    dataRecord.TryGetNameForId(itemId, out selected);
                }
                return selected;
            }
            set
            {
                if (dataRecord && dataRecord.TryGetIdForName(value, out int id))
                {
                    itemId = id;
                }
            }
        }

        protected List<string> ItemNameOptions
        {
            get
            {
                if (availableItemNames == null || (DataRecordValid && availableItemNames.Count != dataRecord.ItemCount))
                {
                    TryPopulateItemNames();
                }
                return availableItemNames;
            }
        }

        private bool editor_RecordRequiresExtraData => dataRecord != null && dataRecord.ExtraDataRequiredAtRuntime;
        private bool editor_SourceInstanceNeedsSet => editor_RecordRequiresExtraData && resolutionType == DataSourceIdResolutionType.ManuallySetInstance;

        /// <summary>
        /// We're going to want to display strings for the items on the Inspector UI
        /// </summary>
        private List<string> availableItemNames = null;
        public string ItemName { get; private set; }

        public virtual void Bind()
        {
            Subscribe();
        }

        public virtual void Unbind()
        {
            Unsubscribe();
        }

        protected void Subscribe()
        {
            // TODO: handle more complex resolution of SourceId for local VMs
            DataSourceManager.SubscribeToItem(dataRecord.SourceId, itemId, OnDataItemUpdate);
        }

        protected void Unsubscribe()
        {
            DataSourceManager.UnsubscribeFromItem(dataRecord.SourceId, itemId, OnDataItemUpdate);
        }

        public virtual void OnDataItemUpdate(IDataSource dataSource, int itemId)
        {
            if (dataSource.TryGetItem<T>(itemId, out T itemValue))
            {
                OnDataUpdated(itemValue);
            }
        }

        protected bool TryGetDataSource(out IDataSource dataSource)
        {
            return DataSourceManager.TryGetDataSource(dataRecord.SourceId, out dataSource);
        }

        protected abstract void OnDataUpdated(T dataValue);

        private void TryPopulateItemNames()
        {
            if (availableItemNames == null)
            {
                availableItemNames = new List<string>();
            }

            availableItemNames.Clear();

            dataRecord.PopulateItemNameList(availableItemNames);

            if (dataRecord.TryGetNameForId(itemId, out string name))
            {
                SelectedItemName = name;
            }
            else
            {
                // insert an empty string at the beginning of the list if no valid option has been selected
                availableItemNames.Insert(0, string.Empty);
            }
        }
    }
}