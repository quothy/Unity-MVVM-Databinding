using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public interface IDataItem
    {
        int Id { get; }
        string Name { get; }

        Type DataType { get; }

        void Initialize(int id, string name);
    }

    [Serializable]
    public abstract class DataItem : IDataItem
    {
        public int Id { get; protected set; }

        public string Name { get; protected set; }

        public abstract Type DataType { get; }

        public event Action ValueChanged = null;

        public void Initialize(int id, string name)
        {
            Id = id;
            Name = name;
        }

        protected void RaiseValueChanged()
        {
            ValueChanged?.Invoke();
        }
    }

    public abstract class DataItem<T> : DataItem
    {
        public override Type DataType => typeof(T);

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

    /// <summary>
    /// In order for DataItem<T> types to be serialized as SerializeReferences in the 
    /// prefab YAML, they must have a concrete type. Below is a list of common concrete types
    /// that are available by default.
    /// 
    /// Any built in types that get added here must also be added to <see cref="DataItemTypeCache.RegisterBuiltInTypes"/>.
    /// </summary>
    public class DataItemInt : DataItem<int> { }
    public class DataItemFloat : DataItem<float> { }
    public class DataItemBool : DataItem<bool> { }
    public class DataItemString : DataItem<string> { }
}
