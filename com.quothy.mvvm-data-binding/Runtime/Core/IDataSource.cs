using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public delegate void DataItemUpdate(IDataSource source, int id);

    public interface IDataSource
    {
        void GenerateRecord();

        void LoadDataRecord(DataRecord record);

        void SubscribeToItem(int id, DataItemUpdate onUpdate);

        void UnsubscribeFromItem(int id, DataItemUpdate onUpdate);

        bool TryGetItem<T>(int id, out T item);

        bool TrySetItem<T>(int id, T item);
    }
}
