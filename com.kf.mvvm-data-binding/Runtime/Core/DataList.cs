// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;

namespace MVVMDatabinding
{
    public abstract class DataList
    {
        public event Action ListUpdated;
        public event Action<int> SelectedIndexChanged;
        public abstract int Count { get; }

        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    SelectedIndexChanged?.Invoke(selectedIndex);
                }
            }
        }

        protected void OnListUpdated()
        {
            ListUpdated?.Invoke();
        }
    }

    public class DataList<T> : DataList
    {
        private List<T> dataList = null;

        public override int Count => dataList.Count;

        public DataList(int capacity = 10)
        {
            dataList = new List<T>(capacity);
        }

        public void Add(T item)
        {
            dataList.Add(item);
            OnListUpdated();
        }

        public void AddRange(IEnumerable<T> range)
        {
            dataList.AddRange(range);
            OnListUpdated();
        }

        public void Clear()
        {
            dataList.Clear();
            OnListUpdated();
        }

        public T this[int key]
        {
            get => dataList[key];
            set 
            { 
                dataList[key] = value;
                OnListUpdated();
            }
        }
        
        // Optionally, you can add a strongly-typed SelectedItem property for convenience:
        public T SelectedItem
        {
            get => (SelectedIndex >= 0 && SelectedIndex < dataList.Count) ? dataList[SelectedIndex] : default;
        }
    }
}