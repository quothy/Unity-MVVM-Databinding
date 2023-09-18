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
        protected ThemeRecord themeRecord = null;

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

        public event Action DataUpdated = null;

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
                    themeRecord.TryGetInfoForId(itemId, out selected, out ThemeItemType unused);
                    name = selected;
                }
                return selected;
            }
            set
            {
                if (themeRecord && themeRecord.TryGetInfoForName(value, out int id, out ThemeItemType unused))
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

            themeRecord.PopulateItemNameList(availableItemNames);

            if (themeRecord.TryGetInfoForId(itemId, out string name, out ThemeItemType unused))
            {
                SelectedItemName = name;
            }
            else
            {
                // insert an empty string at the beginning of the list if no valid option has been selected
                availableItemNames.Insert(0, string.Empty);
            }
        }

        public void Subscribe()
        {
            int sourceId = ThemeManager.GetThemeSourceId(themeRecord.RecordName);
            DataSourceManager.SubscribeToItem(sourceId, itemId, OnDataItemUpdate);
        }

        public void Unsubscribe()
        {
            int sourceId = ThemeManager.GetThemeSourceId(themeRecord.RecordName);
            DataSourceManager.UnsubscribeFromItem(sourceId, itemId, OnDataItemUpdate);
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
            int sourceId = ThemeManager.GetThemeSourceId(themeRecord.RecordName);
            return DataSourceManager.TryGetDataSource(sourceId, out dataSource);
        }
    }
}