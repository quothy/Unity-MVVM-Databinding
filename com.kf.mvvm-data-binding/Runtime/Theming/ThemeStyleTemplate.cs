using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [CreateAssetMenu(fileName = "ThemeStyleTemplate", menuName = "MVVM/Theming/Theme Style Template")]
    public class ThemeStyleTemplate : ScriptableObject
    {
        [Serializable]
        public class ThemeTemplateItem
        {
            [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
            public int Id = int.MinValue;
            public string Name = string.Empty;
            public ThemeItemType Type = ThemeItemType.None;
            public bool ExcludeFromVariants = false;
        }

        [SerializeField]
        private List<ThemeTemplateItem> templateItems = null;

        public List<ThemeTemplateItem> TemplateItems => templateItems;
        public int ItemCount => templateItems.Count;
#if UNITY_EDITOR
        private List<int> usedIds = new List<int>();
#endif

        public void PopulateItemNameList(List<string> itemNames)
        {
            foreach (ThemeTemplateItem item in templateItems)
            {
                itemNames.Add(item.Name);
            }
        }

        public void PopulateItemNameListForType(List<string> itemNames, ThemeItemType type)
        {
            foreach (ThemeTemplateItem item in templateItems)
            {
                if (item.Type == type)
                {
                    itemNames.Add(item.Name);
                }
            }
        }

        public bool TryGetInfoForName(string name, out int id, out ThemeItemType itemType, out bool excludeFromVariants)
        {
            id = int.MinValue;
            itemType = ThemeItemType.None;
            excludeFromVariants = false;
            foreach (ThemeTemplateItem item in templateItems)
            {
                if (name == item.Name)
                {
                    id = item.Id;
                    itemType = item.Type;
                    excludeFromVariants = item.ExcludeFromVariants;
                    break;
                }
            }
            return id != int.MinValue;
        }

        public bool TryGetInfoForId(int id, out string name, out ThemeItemType itemType, out bool excludeFromVariants)
        {
            name = string.Empty;
            itemType = ThemeItemType.None;
            excludeFromVariants = false;
            foreach (ThemeTemplateItem item in templateItems)
            {
                if (id == item.Id)
                {
                    name = item.Name;
                    itemType = item.Type;
                    excludeFromVariants = item.ExcludeFromVariants;
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
            foreach (var item in templateItems)
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