// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding
{
    public abstract class BaseDataSource : IDataSource
    {
        protected Dictionary<int, IDataItem> dataItemLookup = null;
        private Dictionary<int, List<DataItemUpdate>> subscriberLookup = null;

        private string name = string.Empty;
        public string Name => name;

        private int nameHash = 0;
        public int Id => nameHash;

        private bool idModifiedAtRuntime = false;
        public bool IdModifiedAtRuntime => idModifiedAtRuntime;

        private HashSet<int> activeUpdateIds = new HashSet<int>(20);
        private Dictionary<int, List<DataItemUpdate>> pendingSubscriptionLookup = new Dictionary<int, List<DataItemUpdate>>(20);
        private Dictionary<int, List<DataItemUpdate>> pendingUnsubscriptionLookup = new Dictionary<int, List<DataItemUpdate>>(20);

        public virtual void Initialize(string sourceName, bool idModifiedAtRuntime)
        {
            name = sourceName;
            nameHash = Animator.StringToHash(name);
            // TODO: how to differentiate different instances of the same source type? instance ID?
            // -- We'll at least start by tracking whether or not the name/ID gets modified at runtime or not
            this.idModifiedAtRuntime = idModifiedAtRuntime;

            if (idModifiedAtRuntime)
            {
                Debug.Log($"[BaseDataSource] Registering {sourceName} to id {nameHash}");
            }

            if (Application.isPlaying)
            {
                DataSourceManager.RegisterDataSource(this);
            }
        }

        public void Destroy()
        {
            if (Application.isPlaying)
            {
                DataSourceManager.UnregisterDataSource(this);
            }
        }

        public void GenerateRecord(string recordDirPath, List<IDataItem> dataItems, string typeName = "")
        {
#if UNITY_EDITOR
            // ensure directory exists
            try { Directory.CreateDirectory(recordDirPath); } catch { }

            string recordPath = $"{recordDirPath}/{name}_DataRecord.asset";

            DataRecord record = null;
            try
            {
                record = AssetDatabase.LoadAssetAtPath<DataRecord>(recordPath);
                if (record == null)
                {
                    record = ScriptableObject.CreateInstance<DataRecord>();
                    AssetDatabase.CreateAsset(record, recordPath);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("[BaseDataSource] Error creating or loading DataRecord asset at {0}. Exception: {1}", recordPath, ex.Message);
            }

            if (record != null)
            {
                record.PopulateRecord(Id, Name, IdModifiedAtRuntime, dataItems, typeName);
                EditorUtility.SetDirty(record);
                AssetDatabase.SaveAssetIfDirty(record);
            }
            else
            {
                Debug.LogError($"[BaseDataSource] Failed to create or load DataRecord at {recordPath}");
            }

#endif
        }

        public virtual void LoadDataRecord(DataRecord record)
        {
            dataItemLookup = new Dictionary<int, IDataItem>(record.ItemCount);
            subscriberLookup = new Dictionary<int, List<DataItemUpdate>>(record.ItemCount);

            // TODO: load in 
        }

        public void AddItem(IDataItem item)
        {
            if (dataItemLookup == null)
            {
                dataItemLookup = new Dictionary<int, IDataItem>();
            }

            if (!dataItemLookup.TryGetValue(item.Id, out IDataItem existing))
            {
                dataItemLookup[item.Id] = item;

                if (subscriberLookup != null && subscriberLookup.TryGetValue(item.Id, out List<DataItemUpdate> subscribers))
                {
                    foreach (DataItemUpdate itemUpdate in subscribers)
                    {
                        itemUpdate?.Invoke(this, item.Id);
                    }
                }
            }
            else
            {
                Debug.LogError($"[BaseDataSource] An item with the ID {item.Id} already exists in data source {Name}");
            }
        }

        public void OnItemChangedInSource(int id)
        {
            if (subscriberLookup != null && subscriberLookup.TryGetValue(id, out List<DataItemUpdate> subscribers))
            {
                activeUpdateIds.Add(id);
                foreach (DataItemUpdate item in subscribers)
                {
                    item?.Invoke(this, id);
                }
                activeUpdateIds.Remove(id);

                if (pendingSubscriptionLookup.TryGetValue(id, out List<DataItemUpdate> pendingList))
                {
                    foreach (DataItemUpdate onUpdate in pendingList)
                    {
                        // Otherwise, we add it to the pending list for later processing
                        subscriberLookup[id].Add(onUpdate);

                        if (dataItemLookup != null && dataItemLookup.TryGetValue(id, out IDataItem item))
                        {
                            onUpdate?.Invoke(this, id);
                        }
                    }
                    pendingList.Clear();
                }

                if (pendingUnsubscriptionLookup.TryGetValue(id, out List<DataItemUpdate> pendingUnsubList))
                {
                    foreach (DataItemUpdate onUpdate in pendingUnsubList)
                    {
                        if (subscriberLookup.TryGetValue(id, out List<DataItemUpdate> list))
                        {
                            list.Remove(onUpdate);
                        }
                    }
                    pendingUnsubList.Clear();
                }
            }
        }

        public void SubscribeToItem(int id, DataItemUpdate onUpdate)
        {
            if (subscriberLookup == null)
            {
                subscriberLookup = new Dictionary<int, List<DataItemUpdate>>();
            }

            if (!subscriberLookup.TryGetValue(id, out List<DataItemUpdate> list))
            {
                subscriberLookup[id] = new List<DataItemUpdate>(20);
            }

            if (!pendingSubscriptionLookup.TryGetValue(id, out List<DataItemUpdate> pendingList))
            {
                pendingSubscriptionLookup[id] = new List<DataItemUpdate>(20);
            }

            if (!pendingUnsubscriptionLookup.TryGetValue(id, out List<DataItemUpdate> pendingUnsubList))
            {
                pendingUnsubscriptionLookup[id] = new List<DataItemUpdate>(20);
            }

            if (activeUpdateIds.Contains(id))
            {
                // If the item is already active, we can directly invoke the update
                pendingSubscriptionLookup[id].Add(onUpdate);
            }
            else
            {
                // Otherwise, we add it to the pending list for later processing
                subscriberLookup[id].Add(onUpdate);

                if (dataItemLookup != null && dataItemLookup.TryGetValue(id, out IDataItem item))
                {
                    onUpdate?.Invoke(this, id);
                }
            }
        }

        public void UnsubscribeFromItem(int id, DataItemUpdate onUpdate)
        {
            if (subscriberLookup != null && subscriberLookup.TryGetValue(id, out List<DataItemUpdate> list))
            {
                if (activeUpdateIds.Contains(id))
                {
                    pendingUnsubscriptionLookup[id].Add(onUpdate);
                }
                else
                {
                    // If the item is not active, we can remove it directly from the subscriber list
                    list.Remove(onUpdate);
                }
            }
        }

        public bool TryGetItem<T>(int id, out T item)
        {
            bool success = false;
            item = default(T);

            if (dataItemLookup.TryGetValue(id, out var dataItem) && dataItem is DataItem<T> typedItem)
            {
                item = typedItem.Value;
                success = true;
            }

            return success;
        }

        public bool TrySetItem<T>(int id, T item)
        {
            bool success = false;

            if (dataItemLookup.TryGetValue(id, out var dataItem) && dataItem is DataItem<T> typedItem)
            {
                typedItem.Value = item;
                success = true;
            }

            return success;
        }

        public bool TryGetItemAtIndex<T>(int id, int index, out T item)
        {
            bool success = false;
            item = default(T);

            if (dataItemLookup.TryGetValue(id, out IDataItem dataItem) && dataItem is DataItemList listItem && listItem.Value is DataList<T> typedList)
            {
                if (typedList.Count > index)
                {
                    item = typedList[index];
                    success = true;
                }
            }

            return success;
        }

        public bool TrySetItemAtIndex<T>(int id, int index, T itemValue)
        {
            bool success = false;
            if (dataItemLookup.TryGetValue(id, out IDataItem dataItem) && dataItem is DataItemList listItem && listItem.Value is DataList<T> typedList)
            {
                if (typedList.Count > index)
                {
                    typedList[index] = itemValue;
                    success = true;

                    // TODO: how to handle notifying that list contents have changed?
                }
            }

            return success;
        }

        public static string ResolveNameWithRuntimeId(string sourceName, int runtimeId)
        {
            return $"{sourceName}:{runtimeId}";
        }
    }
}