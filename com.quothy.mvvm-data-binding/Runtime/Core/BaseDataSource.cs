using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public abstract class BaseDataSource : IDataSource
    {
        private Dictionary<int, DataItem> dataItemLookup = null;
        private Dictionary<int, List<DataItemUpdate>> subscriberLookup = null;

        public void GenerateRecord()
        {
            // TODO: implement DataRecord scriptable object creation/population/saving here

        }

        public void LoadDataRecord(DataRecord record)
        {
            dataItemLookup = new Dictionary<int, DataItem>(record.DataItems.Count);
            subscriberLookup = new Dictionary<int, List<DataItemUpdate>>(record.DataItems.Count);

            // TODO: load in 
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
            if ()
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