// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [Serializable]
    public class DataResolver
    {
        [SerializeField]
        protected DataRecord dataRecord = null;

        [ConditionalVisibility(nameof(editor_RecordRequiresExtraData), ConditionResultType.ShowIfEquals)]
        [SerializeField]
        protected DataSourceIdResolutionType resolutionType = DataSourceIdResolutionType.None;

        [ConditionalVisibility(nameof(editor_SourceInstanceNeedsSet), ConditionResultType.ShowIfEquals)]
        [SerializeField]
        protected GameObject dataSourceInstance = null;

       // [ConditionalVisibility(nameof(DataRecordValid), ConditionResultType.ShowIfEquals)]
        [DropdownSelection(nameof(ItemNameOptions), nameof(SelectedItemName))]
        [SerializeField]
        protected int itemId = -1;

        [ConditionalVisibility("", ConditionResultType.Never)]
        public string name = string.Empty;

        // TODO: add label attribute to show the comment as a multiline label
        [ConditionalVisibility(nameof(editor_IsCommentEmpty), ConditionResultType.ShowIfNotEquals)]
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        [SerializeField]
        private string comment = string.Empty;

        private bool editor_IsCommentEmpty => string.IsNullOrWhiteSpace(comment);

        /// <summary>
        /// We're going to want to display strings for the items on the Inspector UI
        /// </summary>
        private List<string> availableItemNames = null;

        private GameObject bindingObject = null;

        public event Action DataUpdated = null;

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
                    dataRecord.TryGetCommentForId(itemId, out comment);
                    name = selected;
                }
                return selected;
            }
            set
            {
                if (dataRecord && dataRecord.TryGetIdForName(value, out int id))
                {
                    itemId = id;
                    name = value;
                    dataRecord.TryGetCommentForId(id, out comment);
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

        private int fullSourceId = int.MinValue;

        protected int SourceId
        {
            get
            {
                if (fullSourceId == int.MinValue && TryResolveDataSourceId(out int sourceId))
                {
                    fullSourceId = sourceId;
                }
                return fullSourceId;
            }
        }

        private bool editor_RecordRequiresExtraData => dataRecord != null && dataRecord.ExtraDataRequiredAtRuntime;
        private bool editor_SourceInstanceNeedsSet => editor_RecordRequiresExtraData && resolutionType == DataSourceIdResolutionType.ManuallySetInstance;

        private void TryPopulateItemNames()
        {
            if (availableItemNames == null)
            {
                availableItemNames = new List<string>();
            }

            availableItemNames.Clear();

            if (!DataRecordValid)
            {
                return;
            }

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

        public void Subscribe(GameObject gameObject)
        {
            bindingObject = gameObject;
            DataSourceManager.SubscribeToItem(SourceId, itemId, OnDataItemUpdate);
        }

        public void Unsubscribe()
        {
            DataSourceManager.UnsubscribeFromItem(SourceId, itemId, OnDataItemUpdate);
        }

        public bool TryGetBindingInfo(out int sourceId, out int itemId)
        {
            sourceId = SourceId;
            itemId = this.itemId;
            return sourceId != int.MinValue && itemId != -1;
        }

        public bool TryGetData<T>(out T data)
        {
            if (TryGetDataSource(out IDataSource source))
            {
                return source.TryGetItem<T>(itemId, out data);
            }
            data = default(T);
            return false;
        }

        public bool TrySetData<T>(T data)
        {
            if (TryGetDataSource(out IDataSource source))
            {
                return source.TrySetItem<T>(itemId, data);
            }

            return false;
        }

        public virtual void OnDataItemUpdate(IDataSource dataSource, int itemId)
        {
            DataUpdated?.Invoke();
        }

        protected bool TryGetDataSource(out IDataSource dataSource)
        {
            return DataSourceManager.TryGetDataSource(SourceId, out dataSource);
        }

        private bool TryResolveDataSourceId(out int sourceId)
        {
            if (!dataRecord.ExtraDataRequiredAtRuntime)
            {
                sourceId = dataRecord.SourceId;
                return true;
            }

            if (resolutionType == DataSourceIdResolutionType.ManuallySetInstance)
            {
                if (dataSourceInstance != null)
                {
                    int id = dataSourceInstance.GetInstanceID();
                    string fullName = BaseDataSource.ResolveNameWithRuntimeId(dataRecord.SourceName, id);
                    sourceId = Animator.StringToHash(fullName);
                    return true;
                }
            }
            else if (resolutionType == DataSourceIdResolutionType.GetComponentInParent)
            {
                Type type = Type.GetType(dataRecord.SourceType);
                if (type != null)
                {
                    var found = bindingObject.GetComponentInParent(type);
                    if (found != null)
                    {
                        dataSourceInstance = found.gameObject;
                        int id = dataSourceInstance.GetInstanceID();
                        string fullName = BaseDataSource.ResolveNameWithRuntimeId(dataRecord.SourceName, id);
                        sourceId = Animator.StringToHash(fullName);
                        return true;
                    }
                }
                else
                {
                    Debug.LogErrorFormat("[BaseBinder] Failed to retrieve ViewModel type from cache for data record (source name: {0}, source type: {1})", dataRecord.SourceName, dataRecord.SourceType);
                }
            }

            sourceId = int.MinValue;
            return false;
        }

    }
}