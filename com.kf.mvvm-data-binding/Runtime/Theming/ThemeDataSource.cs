// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    public class ThemeDataSource : IDataSource
    {
        private Dictionary<int, List<DataItemUpdate>> subscriberLookup = new Dictionary<int, List<DataItemUpdate>>(30);
        private Dictionary<int, ThemeItem> dataItemLookup = new Dictionary<int, ThemeItem>(30);

        private string name = string.Empty;
        public string Name => name;

        private int id = int.MinValue;
        public int Id => id;

        private bool idModifiedAtRuntime = false;
        public bool IdModifiedAtRuntime => idModifiedAtRuntime;

        public void AddItem(IDataItem item)
        {
            if (item is ThemeItem themeItem)
            {
                dataItemLookup[item.Id] = themeItem;
                OnItemChangedInSource(item.Id);
            }
        }

        public bool TryGetThemeItem(int id, out ThemeItem item)
        {
            return dataItemLookup.TryGetValue(id, out item);
        }

        public void GenerateRecord(string recordPath, List<IDataItem> dataItems)
        {
            // do nothing
        }

        public void Initialize(string name, bool idModifiedAtRuntime)
        {
            this.name = name;
            this.id = ThemeManager.GetThemeSourceId(name);
            this.idModifiedAtRuntime = idModifiedAtRuntime;

            if (Application.isPlaying)
            {
                DataSourceManager.RegisterDataSource(this);
            }
        }

        public void SetThemeVariant(ThemeVariant variant)
        {
            foreach (var kvp in dataItemLookup)
            {
                kvp.Value.SetThemeVariant(variant);
                OnItemChangedInSource(kvp.Key);
            }
        }

        public void LoadDataRecord(DataRecord record)
        {
            // do nothing
        }

        public void OnItemChangedInSource(int id)
        {            
            if (subscriberLookup.TryGetValue(id, out var subscriberList))
            {
                foreach (var subscriber in subscriberList)
                {
                    subscriber?.Invoke(this, id);
                }
            }
        }

        public void SubscribeToItem(int id, DataItemUpdate onUpdate)
        {
            if (!subscriberLookup.TryGetValue(id, out List<DataItemUpdate> list))
            {
                subscriberLookup[id] = new List<DataItemUpdate>(20);
            }

            subscriberLookup[id].Add(onUpdate);

            if (dataItemLookup != null && dataItemLookup.TryGetValue(id, out ThemeItem item))
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
            if (dataItemLookup.TryGetValue(id, out ThemeItem themeItem))
            {
                return themeItem.TryGetItem<T>(out item);
            }

            item = default(T);
            return false;
        }

        public bool TrySetItem<T>(int id, T item)
        {
            // not needed
            return false;
        }
    }
}