// Copyright (c) 2024 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using TMPro;
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

#if UNITY_EDITOR
        // NOTE: We're only using object here because it's strictly editor-only code.
        object Editor_GetValue();
#endif
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

#if UNITY_EDITOR
        public abstract object Editor_GetValue();
#endif
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

#if UNITY_EDITOR
        public override object Editor_GetValue()
        {
            return excludedFromVariants ? value : light;
        }
#endif
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
    public class ThemeStyleValue
    {
        [SerializeField]
        private ThemeStyleTemplate themeTemplate = null;

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
            get => themeTemplate != null;
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
                if (themeTemplate != null)
                {
                    if (themeTemplate.TryGetInfoForId(itemId, out selected, out itemType, out bool excludeFromVariants))
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
                if (themeTemplate && themeTemplate.TryGetInfoForName(value, out int id, out ThemeItemType type, out bool excludeFromVariants))
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
                themeTemplate.PopulateItemNameList(availableItemNames);
            }

            if (ThemeRecordValid && themeTemplate.TryGetInfoForId(itemId, out string name, out itemType, out bool excludeFromVariants))
            {
                SelectedItemName = name;
            }
            else
            {
                // insert an empty string at the beginning of the list if no valid option has been selected
                availableItemNames.Insert(0, string.Empty);
            }
        }

        public void Editor_SetTemplate(ThemeStyleTemplate template)
        {
            if (template != this.themeTemplate)
            {
                themeTemplate = template;
                // TODO: clear existing values in case it changes?
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
            if (itemType == ThemeItemType.Texture && (themeValue == null || themeValue.DataType != typeof(Texture)))
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

#endif
            return false;
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

#if UNITY_EDITOR
        public bool MatchesItem(ThemeStyleTemplate template, int itemId)
        {
            return template == themeTemplate && itemId == this.itemId;
        }
#endif
    }

    /// <summary>
    /// A ThemeStyle implements values for items defined in a template. There can be 
    /// multiple ThemeStyle objects for each template (e.g.- Button template with primary/secondary/CTA
    /// styles based on it). ThemeStyles define dark/light/high contrast values for selected items defined
    /// in the theme template (e.g.- font color). 
    /// </summary>
    [CreateAssetMenu(fileName = "ThemeStyle", menuName = "MVVM/Theming/Theme Style")]
    public class ThemeStyle : ScriptableObject
    {
        [SerializeField]
        private string styleName = string.Empty;

        [SerializeField]
        private ThemeStyleTemplate template = null;

        [SerializeField]
        private List<ThemeStyleValue> themeStyleValues = null;

        public string StyleName => styleName;
        public ThemeStyleTemplate Template => template;
        public List<ThemeStyleValue> Values => themeStyleValues;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}