using System;
using System.Collections;
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
        public List<DataRecordItem> DataItems = null;

        internal void PopulateRecord(int sourceId, string sourceName, List<DataItem> itemList)
        {
            SourceId = sourceId;
            SourceName = sourceName;

            if (DataItems == null)
            {
                DataItems = new List<DataRecordItem>(itemList.Count);
            }

            DataItems.Clear();

            foreach (DataItem item in itemList)
            {
                DataItems.Add(new DataRecordItem() { Id = item.Id, Name = item.Name });
            }
        }
    }
}