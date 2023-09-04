using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [Serializable]
    public class DataRecordItem
    {
        public int Id = -1;
        public string Name = string.Empty;
    }

    public class DataRecord : ScriptableObject
    {
        public int SourceId = -1;
        public string SourceName = string.Empty;
        public bool ExtraDataRequiredAtRuntime = false;

        [SerializeField]
        private List<DataRecordItem> dataItems = null;

        private Dictionary<int, DataRecordItem> itemLookup = null;
        private Dictionary<string, int> itemNameToIdMap = null;

        public int ItemCount => dataItems.Count;

        internal void PopulateRecord(int sourceId, string sourceName, bool idModifiedAtRuntime, List<IDataItem> itemList)
        {
            SourceId = sourceId;
            SourceName = sourceName;
            ExtraDataRequiredAtRuntime = idModifiedAtRuntime;

            if (dataItems == null)
            {
                dataItems = new List<DataRecordItem>(itemList.Count);
            }

            dataItems.Clear();

            foreach (IDataItem item in itemList)
            {
                dataItems.Add(new DataRecordItem() { Id = item.Id, Name = item.Name });
            }

            if (itemLookup != null)
            {
                itemLookup.Clear();
            }
        }

        public void Initialize()
        {
            // initialize a lookup so we can quickly look up items by ID at runtime
            if (itemLookup == null)
            {
                itemLookup = new Dictionary<int, DataRecordItem>(dataItems.Count);                
            }
            
            if (itemLookup.Count != dataItems.Count)
            {
                foreach (DataRecordItem item in dataItems)
                {
                    itemLookup[item.Id] = item;
                }
            }
        }

        public void PopulateItemNameList(List<string> itemNames)
        {
            foreach (DataRecordItem item in dataItems)
            {
                itemNames.Add(item.Name);
            }
        }

        public bool TryGetIdForName(string name, out int id)
        {
            id = int.MinValue;
            foreach (DataRecordItem item in dataItems)
            {
                if (name == item.Name)
                {
                    id = item.Id;
                    break;
                }
            }
            return id != int.MinValue;
        }

        public bool TryGetNameForId(int id, out string name)
        {
            name = string.Empty;
            foreach (DataRecordItem item in dataItems)
            {
                if (id == item.Id)
                {
                    name = item.Name;
                    break;
                }
            }
            return !string.IsNullOrWhiteSpace(name);
        }
    }
}