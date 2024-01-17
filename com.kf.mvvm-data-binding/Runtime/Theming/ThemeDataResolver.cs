// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [Serializable]
    public class ThemeDataResolver
    {
        [SerializeField]
        protected ThemeStyleTemplate themeTemplate = null;

        [DropdownSelection(nameof(ItemNameOptions), nameof(SelectedItemName))]
        [SerializeField]
        protected int itemId = -1;

        [ConditionalVisibility("", ConditionResultType.Never)]
        public string name = string.Empty;

        // TODO: add label attribute to show the comment as a multiline label
        [ConditionalVisibility(nameof(editor_IsCommentEmpty), ConditionResultType.ShowIfNotEquals)]
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        [SerializeField]
        private string comment = string.Empty;

        private bool editor_IsCommentEmpty => string.IsNullOrWhiteSpace(comment);

        /// <summary>
        /// We're going to want to display strings for the items on the Inspector UI
        /// </summary>
        private List<string> availableItemNames = null;

        private ThemeStyle themeStyle = null;

        public event Action DataUpdated = null;

        public bool DataRecordValid
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
                    themeTemplate.TryGetInfoForId(itemId, out selected, out ThemeItemType unused, out bool excludeFromVariants);
                    name = selected;
                }
                return selected;
            }
            set
            {
                if (themeTemplate && themeTemplate.TryGetInfoForName(value, out int id, out ThemeItemType unused, out bool excludeFromVariants))
                {
                    itemId = id;
                    name = value;
                }
            }
        }

        protected List<string> ItemNameOptions
        {
            get
            {
                if (availableItemNames == null || DataRecordValid)
                {
                    TryPopulateItemNames();
                }
                return availableItemNames;
            }
        }

        private void TryPopulateItemNames()
        {
            if (availableItemNames == null)
            {
                availableItemNames = new List<string>();
            }

            availableItemNames.Clear();

            if (DataRecordValid)
            {
                themeTemplate.PopulateItemNameList(availableItemNames);
            }

            if (DataRecordValid && themeTemplate.TryGetInfoForId(itemId, out string name, out ThemeItemType unused, out bool excludeFromVariants))
            {
                SelectedItemName = name;
            }
            else
            {
                // insert an empty string at the beginning of the list if no valid option has been selected
                availableItemNames.Insert(0, string.Empty);
            }
        }

        public void Subscribe(GameObject go)
        {
            // try to resolve the ThemeStyle via ThemeStyleApplier
            if (themeStyle == null)
            {
                if (go.TryGetComponent<ThemeStyleApplier>(out ThemeStyleApplier applier) && TryGetStyle(applier, out ThemeStyle style))
                {
                    themeStyle = style;
                }
                else
                {
                    applier = go.GetComponentInParent<ThemeStyleApplier>();
                    if (applier != null && TryGetStyle(applier, out style))
                    {
                        themeStyle = style;
                    }
                }
            }
            if (themeStyle == null)
            {
                Debug.LogWarningFormat("[ThemeDataResolver] Unable to resolve theme style for {0}, item {1}", themeTemplate.name, SelectedItemName);
            }
            int sourceId = ThemeManager.GetThemeSourceId(themeStyle.StyleName);
            DataSourceManager.SubscribeToItem(sourceId, itemId, OnDataItemUpdate);
        }

        public void Unsubscribe()
        {
            if (themeStyle != null)
            {
                int sourceId = ThemeManager.GetThemeSourceId(themeStyle.StyleName);
                DataSourceManager.UnsubscribeFromItem(sourceId, itemId, OnDataItemUpdate);
            }
        }

        public bool TryGetData<T>(out T data)
        {
            if (TryGetDataSource(out IDataSource source))
            {
                return source.TryGetItem<T>(itemId, out data);
            }
            data = default(T);
            return false;
        }

        public virtual void OnDataItemUpdate(IDataSource dataSource, int itemId)
        {
            DataUpdated?.Invoke();
        }

        protected bool TryGetDataSource(out IDataSource dataSource)
        {
            int sourceId = ThemeManager.GetThemeSourceId(themeStyle.StyleName);
            return DataSourceManager.TryGetDataSource(sourceId, out dataSource);
        }

        private bool TryGetStyle(ThemeStyleApplier applier, out ThemeStyle style)
        {
            style = null;
            return applier.TryFindStyleForItem(themeTemplate, itemId, out style);
        }
    }
}