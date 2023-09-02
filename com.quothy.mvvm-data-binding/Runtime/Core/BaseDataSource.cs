using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding
{
    public abstract class BaseDataSource : IDataSource
    {
        private Dictionary<int, DataItem> dataItemLookup = null;
        private Dictionary<int, List<DataItemUpdate>> subscriberLookup = null;

        private string name = string.Empty;
        public string Name => name;

        private int nameHash = 0;
        public int Id => nameHash;

        public void Initialize(string sourceName)
        {
            name = sourceName;
            nameHash = name.GetHashCode();
            // TODO: how to differentiate different instances of the same source type? instance ID?
        }

        public void GenerateRecord(string recordDirPath, List<DataItem> dataItems)
        {
#if UNITY_EDITOR
            // ensure directory exists
            try { Directory.CreateDirectory(recordDirPath); } catch { } 

            // TODO: implement DataRecord scriptable object creation/population/saving here
            string recordPath = Path.Combine(recordDirPath, name + "_DataRecord.asset");

            DataRecord record = null;
            try
            {
                if (!File.Exists(recordPath))
                {
                    record = ScriptableObject.CreateInstance<DataRecord>();
                    AssetDatabase.CreateAsset(record, recordPath);
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
                record.PopulateRecord(Id, Name, dataItems);
            }

            EditorUtility.SetDirty(record);
            AssetDatabase.SaveAssetIfDirty(record);
#endif
        }

        public virtual void LoadDataRecord(DataRecord record)
        {
            dataItemLookup = new Dictionary<int, DataItem>(record.DataItems.Count);
            subscriberLookup = new Dictionary<int, List<DataItemUpdate>>(record.DataItems.Count);

            // TODO: load in 
        }

        public void AddItem(DataItem item)
        {
            if (!dataItemLookup.TryGetValue(item.Id, out DataItem existing))
            {
                dataItemLookup[item.Id] = item;
            }
            else
            {
                Debug.LogError($"[BaseDataSource] An item with the ID {item.Id} already exists in data source {Name}");
            }
        }

        public void SubscribeToItem(int id, DataItemUpdate onUpdate)
        {
            if (!subscriberLookup.TryGetValue(id, out List<DataItemUpdate> list))
            {
                subscriberLookup[id] = new List<DataItemUpdate>(20);
            }

            subscriberLookup[id].Add(onUpdate);
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
            item = default(T);

            if (dataItemLookup.TryGetValue(id, out var dataItem) && dataItem is DataItem<T> typedItem)
            {
                typedItem.Value = item;
                success = true;
            }

            return success;
        }
    }
}