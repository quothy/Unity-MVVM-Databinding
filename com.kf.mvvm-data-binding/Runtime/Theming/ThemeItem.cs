// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Reflection;
using UnityEngine.Events;

namespace MVVMDatabinding.Theming
{
    public class ThemeItem : IDataItem
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public Type DataType
        {
            get
            {
                if (themeValue != null)
                {
                    return themeValue.DataType;
                }
                return null;
            }
        }

        public string Comment { get; private set; }

        public UnityEvent ValueChanged { get; private set; } = new UnityEvent();
        public event Action<int> ValueChangedWithId = null;

        private IThemeValue themeValue = null;

        public void EditorInit(UnityEngine.Object dataSourceOwner, PropertyInfo propertyInfo)
        {
            // do nothing
        }

        public void Initialize(int id, string name, string comment = "")
        {
            Id = id;
            Name = name;
            Comment = comment;
        }

        public void RaiseValueChanged()
        {
            ValueChanged?.Invoke();
            ValueChangedWithId?.Invoke(Id);
        }

        public void RuntimeInit(UnityEngine.Object dataSourceOwner)
        {
            // do nothing
        }

        public void SyncItemWithSource()
        {
            // do nothing
        }

        public void SetThemeItemValue(ThemeItemValue themeItemValue)
        {
            if (this.themeValue != null)
            {
                themeValue.ValueChanged.RemoveListener(RaiseValueChanged);
            }

            this.themeValue = themeItemValue.ThemeValue;

            if (themeValue != null)
            {
                themeValue.ValueChanged.AddListener(RaiseValueChanged);
                RaiseValueChanged();
            }
        }

        public void SetThemeVariant(ThemeVariant variant)
        {
            if (themeValue != null)
            {
                themeValue.SetVariant(variant);
            }
        }

        public bool TryGetItem<T>(out T value)
        {
            bool success = false;
            value = default(T);
            if (DataType  == typeof(T) && themeValue is ThemeValue<T> typedValue)
            {
                value = typedValue.Value;
                success = true;
            }
            return success;
        }
    }

}