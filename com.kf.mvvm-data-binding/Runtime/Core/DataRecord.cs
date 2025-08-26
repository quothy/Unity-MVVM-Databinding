// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [Serializable]
    public class DataRecordItem
    {
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public int Id = -1;
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public string Name = string.Empty;
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public string Type = string.Empty;
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public string Comment = string.Empty;
    }

    public class DataRecord : ScriptableObject
    {
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public int SourceId = -1;
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public string SourceName = string.Empty;
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public string SourceType = string.Empty;
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public bool ExtraDataRequiredAtRuntime = false;
        
        [SerializeField]
        private List<DataRecordItem> dataItems = null;

        private Dictionary<int, DataRecordItem> itemLookup = null;
        private Dictionary<string, int> itemNameToIdMap = null;

        public int ItemCount => dataItems.Count;

        internal void PopulateRecord(int sourceId, string sourceName, bool idModifiedAtRuntime, List<IDataItem> itemList, string sourceType = null)
        {
            SourceId = sourceId;
            SourceName = sourceName;
            SourceType = sourceType ?? sourceName;
            ExtraDataRequiredAtRuntime = idModifiedAtRuntime;

            if (dataItems == null)
            {
                dataItems = new List<DataRecordItem>(itemList.Count);
            }

            dataItems.Clear();

            foreach (IDataItem item in itemList)
            {
                dataItems.Add(new DataRecordItem() { Id = item.Id, Name = item.Name, Type = item.DataType.ToString(), Comment = item.Comment });
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

        public void PopulateItemNameListForType(List<string> itemNames, string typeString)
        {
            foreach (DataRecordItem item in dataItems)
            {
                if (item.Type == typeString)
                {
                    itemNames.Add(item.Name);
                }
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

        public bool TryGetCommentForId(int id, out string comment)
        {
            comment = string.Empty;
            foreach (DataRecordItem item in dataItems)
            {
                if (id == item.Id)
                {
                    comment = item.Comment;
                    break;
                }
            }
            return !string.IsNullOrWhiteSpace(comment);
        }
    }
}