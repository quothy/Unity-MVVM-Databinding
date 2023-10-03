// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    public enum ThemeItemType
    {
        None,
        Color,
        Gradient,
        Material,
        Texture,
        Int,
        Float,
        Vector4,
        TMPGradient,
    }

    [Serializable]
    public class ThemeRecordItem
    {
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        public int Id = int.MinValue;
        public string Name = string.Empty;
        public ThemeItemType Type = ThemeItemType.None;
    }

    [CreateAssetMenu(fileName = "ThemeRecord", menuName = "MVVM/Theming/Theme Record")]
    public class ThemeRecord : ScriptableObject
    {
        [SerializeField]
        private string recordName = string.Empty;

        [SerializeField]
        private List<ThemeRecordItem> recordItems = null;

        public string RecordName => recordName;
        public List<ThemeRecordItem> RecordItems => recordItems;

#if UNITY_EDITOR
        private List<int> usedIds = new List<int>();
#endif
        public void PopulateItemNameList(List<string> itemNames)
        {
            foreach (ThemeRecordItem item in recordItems)
            {
                itemNames.Add(item.Name);
            }
        }

        public void PopulateItemNameListForType(List<string> itemNames, ThemeItemType type)
        {
            foreach (ThemeRecordItem item in recordItems)
            {
                if (item.Type == type)
                {
                    itemNames.Add(item.Name);
                }
            }
        }

        public bool TryGetInfoForName(string name, out int id, out ThemeItemType itemType)
        {
            id = int.MinValue;
            itemType = ThemeItemType.None;
            foreach (ThemeRecordItem item in recordItems)
            {
                if (name == item.Name)
                {
                    id = item.Id;
                    itemType = item.Type;
                    break;
                }
            }
            return id != int.MinValue;
        }

        public bool TryGetInfoForId(int id, out string name, out ThemeItemType itemType)
        {
            name = string.Empty;
            itemType = ThemeItemType.None;
            foreach (ThemeRecordItem item in recordItems)
            {
                if (id == item.Id)
                {
                    name = item.Name;
                    itemType = item.Type;
                    break;
                }
            }
            return !string.IsNullOrWhiteSpace(name);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            bool anySet = false;
            usedIds.Clear();
            foreach (var item in recordItems)
            {
                if (item.Id == int.MinValue || usedIds.Contains(item.Id) || item.Id == 0)
                {
                    item.Id = UnityEngine.Random.Range(0, int.MaxValue);
                    UnityEditor.EditorUtility.SetDirty(this);
                    anySet = true;
                }

                usedIds.Add(item.Id);
            }

            if (anySet)
            {
                AssetDatabase.SaveAssetIfDirty(this);
            }
#endif
        }
    }
}