// Copyright (c) 2024 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [Serializable]
    public class ThemeStylePicker
    {
        private const string noStylesMatchingTemplateAvailableMessage = "<No styles of template {0} found>";

        internal event Action<ThemeStylePicker> ThemeStyleChanged = null;

        [SerializeField]
        private ThemeStyleTemplate template = null;

        [ConditionalVisibility(nameof(TemplateValid), ConditionResultType.ShowIfEquals)]
        [DropdownSelection(nameof(StyleNameOptions), nameof(SelectedItemName))]
        [SerializeField]
        private ThemeStyle themeStyle = null;

        protected bool TemplateValid => template != null;

        // TODO: cache these globally so each ThemeStyleApplier doesn't need to do the work?
        private Dictionary<string, ThemeStyle> cachedStyles = new Dictionary<string, ThemeStyle>();
        private List<string> styleNameOptions = null;
        public List<string> StyleNameOptions
        {
            get
            {

                if (styleNameOptions == null || cachedStyles.Count == 0 || !cachedStyles.TryGetValue(styleNameOptions[0], out var style) || style.Template != template)
                {
                    if (styleNameOptions == null)
                    {
                        styleNameOptions = new List<string>();
                    }

                    Editor_PopulateStyleNameOptions();
                }

                // reset the themeStyle to null when changing the template to a different template
                if (themeStyle != null && (cachedStyles.Count == 0 || !styleNameOptions.Contains(themeStyle.StyleName)))
                {
                    themeStyle = null;
                }

                return styleNameOptions;
            }
        }

        private string editor_notAvailableMessage => string.Format(noStylesMatchingTemplateAvailableMessage, template != null ? template.ToString() : "null");
        protected string SelectedItemName
        {
            get
            {
                string selected = string.Empty;
                if (template != null)
                {
                    if (styleNameOptions.Count == 0 || (styleNameOptions.Count == 1 && styleNameOptions[0] == editor_notAvailableMessage))
                    {
                        selected = editor_notAvailableMessage;
                    }
                    else if (themeStyle != null)
                    {
                        selected = themeStyle.StyleName;
                    }
                    else
                    {
                        selected = string.Empty;
                    }

                }
                return selected;
            }
            set
            {
                if (template && cachedStyles.TryGetValue(value, out ThemeStyle style))
                {
                    themeStyle = style;
                    ThemeStyleChanged?.Invoke(this);
                    // TODO: need to propagate the change through any child ThemeBinders
                    // TODO: change ThemeBinders to work with styles & templates and add a way to pass in style-specific info
                    // TODO: should we enable style changes at runtime?
                }
            }
        }

        public ThemeStyle Style => themeStyle;

        public bool Editor_Subscribed => ThemeStyleChanged != null;

        internal void Editor_OnValidate()
        {
            if (styleNameOptions == null)
            {
                styleNameOptions = new List<string>();
            }

            Editor_PopulateStyleNameOptions();
        }

        private void Editor_PopulateStyleNameOptions()
        {
#if UNITY_EDITOR
            string[] styleGuids = AssetDatabase.FindAssets("t:ThemeStyle");
            if (cachedStyles == null)
            {
                cachedStyles = new Dictionary<string, ThemeStyle>(styleGuids.Length);
            }
            styleNameOptions.Clear();
            cachedStyles.Clear();

            foreach (string guid in styleGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ThemeStyle style = AssetDatabase.LoadAssetAtPath<ThemeStyle>(assetPath);
                if (style.Template == template && !cachedStyles.TryGetValue(style.StyleName, out  ThemeStyle unused))
                {
                    cachedStyles.Add(style.StyleName, style);
                    styleNameOptions.Add(style.StyleName);
                }
            }

            if (themeStyle == null && cachedStyles.Count > 0)
            {
                styleNameOptions.Insert(0, string.Empty);
            }
#endif
        }
    }

    public class ThemeStyleApplier : MonoBehaviour
    {
        [SerializeField]
        private List<ThemeStylePicker> themeStyles = null;

        public bool TryFindStyleForItem(ThemeStyleTemplate template, int itemId, out ThemeStyle style)
        {
            style = null;

            foreach (ThemeStylePicker picker in themeStyles)
            {
                if (picker.Style.Template == template)
                {
                    foreach (ThemeStyleValue value in picker.Style.Values)
                    {
                        if (value.Id == itemId)
                        {
                            style = picker.Style;
                            return true;
                        }
                    }
                }
            }

            return style == null;
        }

        private void OnValidate()
        {
            DiscoverThemeBinders(cachedBinders);
            foreach (ThemeStylePicker picker in themeStyles)
            {
                if (!picker.Editor_Subscribed)
                {
                    picker.ThemeStyleChanged += Editor_OnThemeStyleChanged;
                }
                picker.Editor_OnValidate();
                UpdateBinders(picker);
            }
        }

#if UNITY_EDITOR
        private bool subscribedToPickerEvent = false;

        private List<ThemeBinder> cachedBinders = new List<ThemeBinder>();
        public void Editor_OnThemeStyleChanged(ThemeStylePicker picker)
        {
            if (!themeStyles.Contains(picker))
            {
                return;
            }

            DiscoverThemeBinders(cachedBinders);
            UpdateBinders(picker);
        }

        public void Editor_ForceUpdateVisuals(Theme selectedTheme)
        {
            DiscoverThemeBinders(cachedBinders);

            foreach (ThemeBinder cached in cachedBinders)
            {
                foreach (IThemeBinder binder in cached.Binders)
                {
                    binder.Editor_ForceUpdateValueFromTheme(selectedTheme);
                }
            }
        }

        private void UpdateBinders(ThemeStylePicker picker)
        {
            foreach (ThemeBinder cached in cachedBinders)
            {
                foreach (IThemeBinder binder in cached.Binders)
                {
                    binder.Editor_SetStyle(picker.Style);
                }
            }
        }

        private void DiscoverThemeBinders(List<ThemeBinder> binders)
        {
            // need to be able to collect up and cache any sibling or child ThemeBinders in 
            // order to propagate selected style info from ThemeStylePickers
            binders.Clear();

            // check for sibling first
            if (gameObject.TryGetComponent<ThemeBinder>(out ThemeBinder binder))
            {
                binders.Add(binder);
            }

            // then get any child ThemeBinders
            var childBinders = gameObject.GetComponentsInChildren<ThemeBinder>();
            binders.AddRange(childBinders);
        }
#endif
    }
}