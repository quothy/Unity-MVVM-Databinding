using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [Serializable]
    public abstract class DataItem
    {
        public int Id { get; internal set; }

        public string Name { get; internal set; }

        public event Action ValueChanged = null;

        public void RaiseValueChanged()
        {
            ValueChanged?.Invoke();
        }
    }

    public abstract class DataItem<T> : DataItem
    {
        private T value;
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                if (!this.value.Equals(value))
                {
                    this.value = value;
                    // raise changed event
                    RaiseValueChanged();
                }
            }
        }





    }
}
