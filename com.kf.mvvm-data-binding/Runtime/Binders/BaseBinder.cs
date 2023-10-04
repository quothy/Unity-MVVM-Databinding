// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
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
        private const string noDataItemsOfTypeAvailableMessage = "<No items of type {0} to bind to>";

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

        [ConditionalVisibility("", ConditionResultType.Never)]
        public string name = string.Empty;

        [ConditionalShowAsMessage(nameof(editor_IsCommentEmpty), ConditionResultType.ShowIfNotEquals, ConditionalShowAsMessageAttribute.MessageType.Info)]
        [SerializeField]
        private string comment = string.Empty;
        private bool editor_IsCommentEmpty => string.IsNullOrWhiteSpace(comment);

        private string editor_notAvailableMessage => string.Format(noDataItemsOfTypeAvailableMessage, typeof(T).ToString());

        protected string binderTypeName = string.Empty;
        private string BinderTypeName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(binderTypeName))
                {
                    binderTypeName = string.Format(" ({0})", GetType().Name);
                }
                return binderTypeName;
            }
        }

        public string Name => name;

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
                    if (availableItemNames.Count == 0 || (availableItemNames.Count == 1 && availableItemNames[0] == editor_notAvailableMessage))
                    {
                        name = BinderTypeName;
                        selected = editor_notAvailableMessage;
                        comment = string.Empty;
                    }
                    else if (dataRecord.TryGetNameForId(itemId, out selected))
                    {
                        name = selected + BinderTypeName;
                        dataRecord.TryGetCommentForId(itemId, out comment);
                    }
                    
                }
                return selected;
            }
            set
            {
                if (dataRecord && dataRecord.TryGetIdForName(value, out int id))
                {
                    itemId = id;
                    name = value + BinderTypeName;
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
        private GameObject bindingObject = null;


        private bool editor_RecordRequiresExtraData => dataRecord != null && dataRecord.ExtraDataRequiredAtRuntime;
        private bool editor_SourceInstanceNeedsSet => editor_RecordRequiresExtraData && resolutionType == DataSourceIdResolutionType.ManuallySetInstance;

        /// <summary>
        /// We're going to want to display strings for the items on the Inspector UI
        /// </summary>
        private List<string> availableItemNames = null;
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

        public string ItemName { get; private set; }

        public virtual void Bind(GameObject bindingObject)
        {
            this.bindingObject = bindingObject;
            Subscribe();
        }

        public virtual void Unbind()
        {
            Unsubscribe();
        }

        protected void Subscribe()
        {
            DataSourceManager.SubscribeToItem(SourceId, itemId, OnDataItemUpdate);
        }

        protected void Unsubscribe()
        {
            DataSourceManager.UnsubscribeFromItem(SourceId, itemId, OnDataItemUpdate);
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
            return DataSourceManager.TryGetDataSource(SourceId, out dataSource);
        }

        protected abstract void OnDataUpdated(T dataValue);

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
                if (bindingObject != null && ViewModelTypeCache.TryGetViewModelType(dataRecord.SourceType, out Type viewModelType))
                {
                    var source = bindingObject.GetComponentInParent(viewModelType);
                    if (source != null)
                    {
                        int id = source.gameObject.GetInstanceID();
                        string fullName = BaseDataSource.ResolveNameWithRuntimeId(dataRecord.SourceName, id);
                        sourceId = Animator.StringToHash(fullName);
                        Debug.Log($"[BaseBinder] Resolved data source: name = {fullName}  id = {sourceId}");
                        return true;
                    }
                    else
                    {
                        Debug.LogErrorFormat("[BaseBinder] Failed to find ViewModel of type {0} in parent of {1}", viewModelType, bindingObject.name);
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

        private void TryPopulateItemNames()
        {
            if (availableItemNames == null)
            {
                availableItemNames = new List<string>();
            }

            availableItemNames.Clear();

            dataRecord.PopulateItemNameListForType(availableItemNames, typeof(T).ToString());

            if (availableItemNames.Count > 0)
            {
                if (dataRecord.TryGetNameForId(itemId, out string name))
                {
                    SelectedItemName = name;
                }
                else
                {
                    availableItemNames.Insert(0, string.Empty);
                }
            }
            else
            {
                // insert an empty string at the beginning of the list if no valid option has been selected                
                availableItemNames.Insert(0, string.Format(noDataItemsOfTypeAvailableMessage, typeof(T).ToString()));
            }
        }
    }
}