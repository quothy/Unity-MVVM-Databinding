// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace MVVMDatabinding.Theming
{
    public enum ThemeVariant
    {
        Light,
        Dark,
        HighContrast,
    }

    public interface IThemeValue
    {
        Type DataType { get; }

        UnityEvent ValueChanged { get; }

        bool ExcludedFromVariants { get; set; }

        void SetVariant(ThemeVariant variant);
    }

    [Serializable]
    public abstract class ThemeValue : IThemeValue
    {
        protected ThemeVariant activeVariant { get; set; } = ThemeVariant.Light;
        public abstract Type DataType { get; }
        public UnityEvent ValueChanged { get; protected set; } = new UnityEvent();

        [HideInInspector]
        [SerializeField]
        protected bool excludedFromVariants = false;


        public bool ExcludedFromVariants
        {
            get => excludedFromVariants;
            set
            {
                excludedFromVariants = value;
            }
        }

        public void SetVariant(ThemeVariant variant)
        {
            activeVariant = variant;
            ValueChanged?.Invoke();
        }
    }

    public abstract class ThemeValue<T> : ThemeValue
    {
        [ConditionalVisibility(nameof(ExcludedFromVariants), ConditionResultType.ShowIfNotEquals)]
        [SerializeField]
        private T light;

        [ConditionalVisibility(nameof(ExcludedFromVariants), ConditionResultType.ShowIfNotEquals)]
        [SerializeField]
        private T dark;

        [ConditionalVisibility(nameof(ExcludedFromVariants), ConditionResultType.ShowIfNotEquals)]
        [SerializeField]
        private T highContrast;

        [ConditionalVisibility(nameof(ExcludedFromVariants), ConditionResultType.ShowIfEquals)]
        [SerializeField]
        private T value;

        public T Value
        {
            get
            {
                if (ExcludedFromVariants)
                {
                    return value;
                }

                switch (activeVariant)
                {
                    case ThemeVariant.Light:
                        return light;
                    case ThemeVariant.Dark:
                        return dark;
                    case ThemeVariant.HighContrast:
                        return highContrast;
                }

                return default(T);
            }
        }

        public override Type DataType => typeof(T);
    }

    public class ColorThemeValue : ThemeValue<Color> { }
    public class GradientThemeValue : ThemeValue<Gradient> { }
    public class MaterialThemeValue : ThemeValue<Material> { }
    public class TextureThemeValue : ThemeValue<Texture> { }
    public class IntThemeValue : ThemeValue<int> { }
    public class FloatThemeValue : ThemeValue<float> { }
    public class Vector4ThemeValue : ThemeValue<Vector4> { }
    public class TMPGradientThemeValue : ThemeValue<TMP_ColorGradient> { }
    public class FontSettingsThemeValue : ThemeValue<ThemeFontSettings> { }

    [Serializable]
    public class ThemeItemValue
    {
        [SerializeField]
        private ThemeRecord themeRecord = null;

        [DropdownSelection(nameof(ItemNameOptions), nameof(SelectedItemName))]
        [SerializeField]
        private int itemId = int.MinValue;

        [ConditionalVisibility("", ConditionResultType.Never)]
        [SerializeField]
        private string name = string.Empty;

        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        [SerializeField]
        private ThemeItemType itemType = ThemeItemType.None;

        [SerializeReference]
        private IThemeValue themeValue = null;

        public int Id => itemId;
        public string Name => name;

        public bool ThemeRecordValid
        {
            get => themeRecord != null;
        }

        private List<string> availableItemNames = null;
        protected List<string> ItemNameOptions
        {
            get
            {
                if (availableItemNames == null || ThemeRecordValid)
                {
                    TryPopulateItemNames();
                }
                return availableItemNames;
            }
        }
        
        protected string SelectedItemName
        {
            get
            {
                string selected = string.Empty;
                if (themeRecord != null)
                {
                    if (themeRecord.TryGetInfoForId(itemId, out selected, out itemType, out bool excludeFromVariants))
                    {
                        name = selected;
                        UpdateThemeValue();
                        themeValue.ExcludedFromVariants = excludeFromVariants;
                    }
                    else
                    {
                        itemId = 0;
                        name = string.Empty;
                        themeValue = null;
                    }
                }
                else
                {
                    itemType = ThemeItemType.None;
                }
                return selected;
            }
            set
            {
                if (themeRecord && themeRecord.TryGetInfoForName(value, out int id, out ThemeItemType type, out bool excludeFromVariants))
                {
                    itemId = id;
                    name = value;
                    itemType = type;
                    UpdateThemeValue();
                    themeValue.ExcludedFromVariants = excludeFromVariants;
                }
                else
                {
                    itemType = ThemeItemType.None;
                }
            }
        }

        public IThemeValue ThemeValue => themeValue;


        public string RecordName => themeRecord.RecordName;

        public UnityEvent ValueChanged { get; protected set; } = null;

        public void Initialize()
        {
            if (themeValue != null && themeValue is ThemeValue typedValue)
            {
                typedValue.ValueChanged.AddListener(RaiseValueChanged);
            }
        }

        public void RaiseValueChanged()
        {
            ValueChanged?.Invoke();
        }

        private void TryPopulateItemNames()
        {
            if (availableItemNames == null)
            {
                availableItemNames = new List<string>();
            }

            availableItemNames.Clear();

            if (ThemeRecordValid)
            {
                themeRecord.PopulateItemNameList(availableItemNames);
            }

            if (ThemeRecordValid && themeRecord.TryGetInfoForId(itemId, out string name, out itemType, out bool excludeFromVariants))
            {
                SelectedItemName = name;
            }
            else
            {
                // insert an empty string at the beginning of the list if no valid option has been selected
                availableItemNames.Insert(0, string.Empty);
            }
        }


        public bool Editor_OnValidate()
        {
#if UNITY_EDITOR
            // check to make sure that the DataType of themeValue is what we expect it to be
            if (itemType == ThemeItemType.Color && (themeValue == null || themeValue.DataType != typeof(Color)))
            {
                themeValue = new ColorThemeValue();
                return true;
            }            
            if (itemType == ThemeItemType.Gradient && (themeValue == null || themeValue.DataType != typeof(Gradient)))
            {
                themeValue = new GradientThemeValue();
                return true;
            }
            if (itemType == ThemeItemType.Material && (themeValue == null || themeValue.DataType != typeof(Material)))
            {
                themeValue = new MaterialThemeValue();
                return true;
            }
            if (itemType == ThemeItemType.Texture  && (themeValue == null || themeValue.DataType != typeof(Texture)))
            {
                themeValue = new TextureThemeValue();
                return true;
            }
            if (itemType == ThemeItemType.Int && (themeValue == null || themeValue.DataType != typeof(int)))
            {
                themeValue = new IntThemeValue();
                return true;
            }
            if (itemType == ThemeItemType.Float && (themeValue == null || themeValue.DataType != typeof(float)))
            {
                themeValue = new FloatThemeValue();
                return true;
            }
            if (itemType == ThemeItemType.Vector4 && (themeValue == null || themeValue.DataType != typeof(Vector4)))
            {
                themeValue = new Vector4ThemeValue();
                return true;
            }
            if (itemType == ThemeItemType.TMPGradient && (themeValue == null || themeValue.DataType != typeof(TMP_ColorGradient)))
            {
                themeValue = new TMPGradientThemeValue();
                return true;
            }
            if (itemType == ThemeItemType.FontSettings && (themeValue == null || themeValue.DataType != typeof(ThemeFontSettings)))
            {
                themeValue = new FontSettingsThemeValue();
                return true;
            }

            return false;
#endif
        }

        public void UpdateThemeValue()
        {
#if UNITY_EDITOR
            // check to make sure that the DataType of themeValue is what we expect it to be
            if (itemType == ThemeItemType.Color && (themeValue == null || themeValue.DataType != typeof(Color)))
            {
                themeValue = new ColorThemeValue();
            }
            else if (itemType == ThemeItemType.Gradient && (themeValue == null || themeValue.DataType != typeof(Gradient)))
            {
                themeValue = new GradientThemeValue();
            }
            else if (itemType == ThemeItemType.Material && (themeValue == null || themeValue.DataType != typeof(Material)))
            {
                themeValue = new MaterialThemeValue();
            }
            else if (itemType == ThemeItemType.Texture && (themeValue == null || themeValue.DataType != typeof(Texture)))
            {
                themeValue = new TextureThemeValue();
            }
            else if (itemType == ThemeItemType.Int && (themeValue == null || themeValue.DataType != typeof(int)))
            {
                themeValue = new IntThemeValue();
            }
            else if (itemType == ThemeItemType.Float && (themeValue == null || themeValue.DataType != typeof(float)))
            {
                themeValue = new FloatThemeValue();
            }
            else if (itemType == ThemeItemType.Vector4 && (themeValue == null || themeValue.DataType != typeof(Vector4)))
            {
                themeValue = new Vector4ThemeValue();
            }
            else if (itemType == ThemeItemType.TMPGradient && (themeValue == null || themeValue.DataType != typeof(TMP_ColorGradient)))
            {
                themeValue = new TMPGradientThemeValue();
            }
            else if (itemType == ThemeItemType.FontSettings && (themeValue == null || themeValue.DataType != typeof(ThemeFontSettings)))
            {
                themeValue = new FontSettingsThemeValue();
            }
#endif
        }

        public void ResetAndUpdateThemeValue()
        {
            themeValue = null;
            UpdateThemeValue();
        }
    }


    [CreateAssetMenu(fileName = "ThemeValueList", menuName ="MVVM/Theming/Theme Value List")]
    public class ThemeValueList : ScriptableObject
    {
        [SerializeField]
        private List<ThemeItemValue> items = null;

        public List<ThemeItemValue> Items => items;

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                bool saveChanges = false;
                foreach (ThemeItemValue value in items)
                {
                    if (value.Editor_OnValidate())
                    {
                        saveChanges = true;
                    }
                }

                if (saveChanges)
                {
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssetIfDirty(this);
                }
            }
        }
    }
}
