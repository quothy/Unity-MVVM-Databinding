// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [Serializable]
    public abstract class BaseThemeBinder<T> : IThemeBinder
    {
        private const string noDataItemsOfTypeAvailableMessage = "<No items of type {0} to bind to>";

        [SerializeField]
        private ThemeStyleTemplate themeTemplate = null;

        [ConditionalVisibility(nameof(ThemeTemplateValid), ConditionResultType.ShowIfEquals)]
        [DropdownSelection(nameof(ItemNameOptions), nameof(SelectedItemName))]
        [SerializeField]
        protected int itemId = -1;

        [ConditionalVisibility("", ConditionResultType.Never)]
        public string name = string.Empty;

        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        [SerializeField]
        private ThemeStyle themeStyle = null;

        private string editor_notAvailableMessage => string.Format(noDataItemsOfTypeAvailableMessage, typeof(T).ToString());

        protected string binderTypeName = string.Empty;
        private string BinderTypeName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(binderTypeName))
                {
                    binderTypeName = string.Format(" ({0})", GetType().Name);
                }
                return binderTypeName;
            }
        }

        public string Name => name;

        public bool ThemeTemplateValid
        {
            get => themeTemplate != null;
        }

        protected string SelectedItemName
        {
            get
            {
                string selected = string.Empty;
                if (themeTemplate != null)
                {
                    if (availableItemNames.Count == 0 || (availableItemNames.Count == 1 && availableItemNames[0] == editor_notAvailableMessage))
                    {
                        name = BinderTypeName;
                        selected = editor_notAvailableMessage;
                    }
                    else if (themeTemplate.TryGetInfoForId(itemId, out selected, out ThemeItemType type, out bool excludeFromVariants))
                    {
                        name = selected + BinderTypeName;
                    }

                }
                return selected;
            }
            set
            {
                if (themeTemplate && themeTemplate.TryGetInfoForName(value, out int id, out ThemeItemType type, out bool excludeFromVariants))
                {
                    itemId = id;
                    name = value + BinderTypeName;
                }
            }
        }

        protected bool IsBindingValid => ThemeTemplateValid && !string.IsNullOrWhiteSpace(SelectedItemName);

        /// <summary>
        /// We're going to want to display strings for the items on the Inspector UI
        /// </summary>
        private List<string> availableItemNames = null;
        protected List<string> ItemNameOptions
        {
            get
            {
                if (availableItemNames == null || (ThemeTemplateValid && availableItemNames.Count != themeTemplate.TemplateItems.Count))
                {
                    TryPopulateItemNames();
                }
                return availableItemNames;
            }
        }

        protected abstract ThemeItemType ThemeItemType { get; }

        public void Bind()
        {
            // subscribe to the theme item
            int sourceId = ThemeManager.GetThemeSourceId(themeStyle.StyleName);
            DataSourceManager.SubscribeToItem(sourceId, itemId, OnThemeItemUpdate);
        }
        public void Unbind()
        {
            int sourceId = ThemeManager.GetThemeSourceId(themeStyle.StyleName);
            DataSourceManager.UnsubscribeFromItem(sourceId, itemId, OnThemeItemUpdate);
        }

        public void OnThemeItemUpdate(IDataSource dataSource, int itemId)
        {
            if (dataSource.TryGetItem<T>(itemId, out T item))
            {
                OnDataUpdated(item);
            }
        }
        protected abstract void OnDataUpdated(T dataValue);

        private void TryPopulateItemNames()
        {
            if (availableItemNames == null)
            {
                availableItemNames = new List<string>();
            }

            availableItemNames.Clear();

            if (ThemeTemplateValid)
            {
                themeTemplate.PopulateItemNameListForType(availableItemNames, ThemeItemType);
            }

            if (availableItemNames.Count > 0)
            {
                if (ThemeTemplateValid && themeTemplate.TryGetInfoForId(itemId, out string name, out var unused, out bool excludeFromVariants))
                {
                    SelectedItemName = name;
                }
                else
                {
                    availableItemNames.Insert(0, string.Empty);
                }
            }
            else
            {
                // insert an empty string at the beginning of the list if no valid option has been selected                
                availableItemNames.Insert(0, string.Format(noDataItemsOfTypeAvailableMessage,ThemeItemType.ToString()));
            }
        }

#if UNITY_EDITOR
        public ThemeStyleTemplate Template => themeTemplate;
        public int ItemId => itemId;

        public void Editor_ForceUpdateItemValue(object value)
        {
            OnDataUpdated((T)value);
        }

        public void Editor_ForceUpdateValueFromTheme(Theme theme)
        {
            foreach (ThemeStyle style in theme.ThemeStyleList)
            {
                if (style.Template == Template && style == themeStyle)
                {
                    foreach (var value in style.Values)
                    {
                        if (value.Id == this.itemId)
                        {
                            OnDataUpdated((T)value.ThemeValue.Editor_GetValue());
                            break;
                        }
                    }
                }
            }
        }

        public void Editor_SetStyle(ThemeStyle style)
        {
            if (style == null || style.Template != Template)
            {
                return;
            }

            foreach (ThemeStyleValue value in style.Values)
            {
                if (value.Id == itemId)
                {
                    themeStyle = style;
                    // TODO: force save asset?
                }
            }
        }
#endif
    }
}