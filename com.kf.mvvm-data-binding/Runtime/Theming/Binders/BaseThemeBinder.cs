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
        private ThemeRecord themeRecord = null;

        [ConditionalVisibility(nameof(DataRecordValid), ConditionResultType.ShowIfEquals)]
        [DropdownSelection(nameof(ItemNameOptions), nameof(SelectedItemName))]
        [SerializeField]
        protected int itemId = -1;

        [ConditionalVisibility("", ConditionResultType.Never)]
        public string name = string.Empty;

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

        public bool DataRecordValid
        {
            get => themeRecord != null;
        }

        protected string SelectedItemName
        {
            get
            {
                string selected = string.Empty;
                if (themeRecord != null)
                {
                    if (availableItemNames.Count == 0 || (availableItemNames.Count == 1 && availableItemNames[0] == editor_notAvailableMessage))
                    {
                        name = BinderTypeName;
                        selected = editor_notAvailableMessage;
                    }
                    else if (themeRecord.TryGetInfoForId(itemId, out selected, out ThemeItemType type, out bool excludeFromVariants))
                    {
                        name = selected + BinderTypeName;
                    }

                }
                return selected;
            }
            set
            {
                if (themeRecord && themeRecord.TryGetInfoForName(value, out int id, out ThemeItemType type, out bool excludeFromVariants))
                {
                    itemId = id;
                    name = value + BinderTypeName;
                }
            }
        }

        /// <summary>
        /// We're going to want to display strings for the items on the Inspector UI
        /// </summary>
        private List<string> availableItemNames = null;
        protected List<string> ItemNameOptions
        {
            get
            {
                if (availableItemNames == null || (DataRecordValid && availableItemNames.Count != themeRecord.RecordItems.Count))
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
            int sourceId = ThemeManager.GetThemeSourceId(themeRecord.RecordName);
            DataSourceManager.SubscribeToItem(sourceId, itemId, OnThemeItemUpdate);
        }
        public void Unbind()
        {
            int sourceId = ThemeManager.GetThemeSourceId(themeRecord.RecordName);
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

            if (DataRecordValid)
            {
                themeRecord.PopulateItemNameListForType(availableItemNames, ThemeItemType);
            }

            if (availableItemNames.Count > 0)
            {
                if (DataRecordValid && themeRecord.TryGetInfoForId(itemId, out string name, out var unused, out bool excludeFromVariants))
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
    }
}