// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

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

        bool IdModifiedAtRuntime { get; }

        void Initialize(string name, bool idModifiedAtRuntime);

        void GenerateRecord(string recordPath, List<IDataItem> dataItems, string typeName = "");

        void LoadDataRecord(DataRecord record);

        void AddItem(IDataItem item);

        void OnItemChangedInSource(int id);

        void SubscribeToItem(int id, DataItemUpdate onUpdate);

        void UnsubscribeFromItem(int id, DataItemUpdate onUpdate);

        bool TryGetItem<T>(int id, out T item);

        bool TrySetItem<T>(int id, T item);

        bool TryGetItemAtIndex<T>(int id, int index, out T item);
        bool TrySetItemAtIndex<T>(int id, int index, T item);
    }
}
