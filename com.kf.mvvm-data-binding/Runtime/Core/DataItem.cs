// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace MVVMDatabinding
{
    public interface IDataItem
    {
        int Id { get; }
        string Name { get; }

        Type DataType { get; }

        string Comment { get; }

        void Initialize(int id, string name, string comment = "");

        UnityEvent ValueChanged { get; }

        void EditorInit(UnityEngine.Object dataSourceOwner, PropertyInfo propertyInfo);
        void RuntimeInit(UnityEngine.Object dataSourceOwner);

        void RaiseValueChanged();

        void SyncItemWithSource();
    }

    [Serializable]
    public abstract class DataItem : IDataItem
    {
        [SerializeField]
        protected int id = -1;

        public int Id
        {
            get => id;
            protected set => id = value;
        }

        [SerializeField]
        protected string name = string.Empty;

        public string Name
        {
            get => name;
            protected set => name = value;
        }

        public abstract Type DataType { get; }

        public string Comment { get; private set; }

        public UnityEvent ValueChanged { get; protected set; } = null;

        public void Initialize(int id, string name, string comment = "")
        {
            Id = id;
            Name = name;
            Comment = comment;
        }

        public void RaiseValueChanged()
        {
            ValueChanged?.Invoke();
        }

        public abstract void EditorInit(UnityEngine.Object dataSourceOwner, PropertyInfo propertyInfo);
        public abstract void RuntimeInit(UnityEngine.Object dataSourceOwner);
        public abstract void SyncItemWithSource();
    }

    public delegate T DataItemGetter<T>();
    public delegate void DataItemSetter<T>(T val);

    public abstract class DataItem<T> : DataItem
    {
        public override Type DataType => typeof(T);

        private T value;
        public T Value
        {
            get
            {
                if (value == null && valueGetter != null)
                {
                    value = valueGetter.Invoke();
                }
                return value;
            }
            set
            {
                if (this.value == null || !this.value.Equals(value))
                {
                    this.value = value;
                    //OnSetValue();
                    setUnderlyingValue?.Invoke(value);
                    // raise changed event
                    RaiseValueChanged();
                }
            }
        }

        protected DataItemGetter<T> valueGetter = null;
        protected DataItemSetter<T> valueSetter = null;

        [SerializeField]
        private UnityEvent<T> getAction = null;


        [SerializeField]
        private UnityEvent<T> setUnderlyingValue = null;
        //[SerializeField]
        //private UnityAction<T> setAction = null;

        public override void EditorInit(UnityEngine.Object dataSourceOwner, PropertyInfo propertyInfo)
        {
#if UNITY_EDITOR
            /// What am I trying to do here? 
            /// I want to use persistent UnityEvents to hook to 
            /// the getter and setter of the property so that I can 
            /// use those at runtime to retrieve the value and set the value
            /// The signature of the getter should be T() and the setter should be
            /// void(T)
            /// 

            if (setUnderlyingValue == null)
            {
                setUnderlyingValue = new UnityEvent<T>();
            }

            if (typeof(IList).IsAssignableFrom(typeof(T)))
            {
                return;
            }
            UnityAction<T> setAction = propertyInfo.GetSetMethod().CreateDelegate(typeof(UnityAction<T>), dataSourceOwner) as UnityAction<T>;
            UnityEventTools.AddPersistentListener<T>(setUnderlyingValue, setAction);
#endif       
        }

        public override void RuntimeInit(UnityEngine.Object dataSourceOwner)
        {
            if (Application.isPlaying)
            {
                PropertyInfo propertyInfo = dataSourceOwner.GetType().GetProperty(Name);
                valueGetter = propertyInfo.GetGetMethod().CreateDelegate(typeof(DataItemGetter<T>), dataSourceOwner) as DataItemGetter<T>;
                SyncItemWithSource();
            }
        }

        public override void SyncItemWithSource()
        {
            if (valueGetter != null)
            {
                Value = valueGetter.Invoke();
            }
        }

        private void OnSetValue()
        {
            T sourceValue = valueGetter.Invoke();
            if (sourceValue == null || !sourceValue.Equals(Value))
            {
                valueSetter?.Invoke(Value);
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
    public class DataItemLong : DataItem<long> { }
    public class DataItemFloat : DataItem<float> { }
    public class DataItemBool : DataItem<bool> { }
    public class DataItemString : DataItem<string> { }
    public class DataItemColor : DataItem<Color> { }
    public class DataItemMaterial : DataItem<Material> { }
    public class DataItemSprite : DataItem<Sprite> { }
    public class DataItemTexture : DataItem<Texture> { }

    public class DataItemVector2 : DataItem<Vector2> { }
    public class DataItemVector3 : DataItem<Vector3> { }

    public class DataItemVector4 : DataItem<Vector4> { }

    public class DataItemList : DataItem<DataList> 
    {
        public override void EditorInit(UnityEngine.Object dataSourceOwner, PropertyInfo propertyInfo)
        {
            // skip setting up setter logic for now 
        }

        public override void RuntimeInit(UnityEngine.Object dataSourceOwner)
        {
            base.RuntimeInit(dataSourceOwner);

            Value.ListUpdated += RaiseValueChanged;
        }
    }

    public class DataItemAction : DataItem<Action> 
    {
        private Action dataAction = null;
        public override void RuntimeInit(UnityEngine.Object dataSourceOwner)
        {
            if (Application.isPlaying)
            {
                MethodInfo methodInfo = dataSourceOwner.GetType().GetMethod(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                dataAction = methodInfo.CreateDelegate(typeof(Action), dataSourceOwner) as Action;
                valueGetter = GetAction;
            }
        }

        private Action GetAction()
        {
            return dataAction;
        }
    }
}
