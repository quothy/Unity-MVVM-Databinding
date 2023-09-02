using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public delegate void DataItemUpdate(IDataSource source, int id);

    public interface IDataSource
    {
        string Name { get; }
        int Id { get; }

        void Initialize(string name);

        void GenerateRecord(string recordPath, List<DataItem> dataItems);

        void LoadDataRecord(DataRecord record);

        void AddItem(DataItem item);

        void SubscribeToItem(int id, DataItemUpdate onUpdate);

        void UnsubscribeFromItem(int id, DataItemUpdate onUpdate);

        bool TryGetItem<T>(int id, out T item);

        bool TrySetItem<T>(int id, T item);
    }
}
