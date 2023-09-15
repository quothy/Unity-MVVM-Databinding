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

        public void GenerateRecord(string recordDirPath, List<IDataItem> dataItems)
        {
#if UNITY_EDITOR
            // ensure directory exists
            try { Directory.CreateDirectory(recordDirPath); } catch { } 

            // TODO: implement DataRecord scriptable object creation/population/saving here
            string recordPath = $"{recordDirPath}/{name}_DataRecord.asset";

            DataRecord record = null;
            try
            {
                if (!File.Exists(recordPath))
                {
                    record = ScriptableObject.CreateInstance<DataRecord>();
                    AssetDatabase.CreateAsset(record, recordPath.Trim());
                }
                else
                {
                    record = AssetDatabase.LoadAssetAtPath(recordPath, typeof(DataRecord)) as DataRecord;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("[BaseDataSource] Error creating or loading DataRecord asset at {0}. Exception: {1}", recordPath, ex.Message);
            }

            if (record != null)
            {
                record.PopulateRecord(Id, Name, IdModifiedAtRuntime, dataItems);
            }

            EditorUtility.SetDirty(record);
            AssetDatabase.SaveAssetIfDirty(record);
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
                foreach (DataItemUpdate item in subscribers)
                {
                    item?.Invoke(this, id);
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

            subscriberLookup[id].Add(onUpdate);

            if (dataItemLookup != null && dataItemLookup.TryGetValue(id, out IDataItem item))
            {
                onUpdate?.Invoke(this, id);
            }
        }

        public void UnsubscribeFromItem(int id, DataItemUpdate onUpdate)
        {
            if (subscriberLookup.TryGetValue(id, out List<DataItemUpdate> list))
            {
                list.Remove(onUpdate);
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

        public static string ResolveNameWithRuntimeId(string sourceName, int runtimeId)
        {
            return $"{sourceName}:{runtimeId}";
        }
    }
}