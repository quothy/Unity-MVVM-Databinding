using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    public enum ThemeVariant
    {
        Light,
        Dark,
        HighContrast,
    }


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

        #region Theme item value fields
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Color)]
        [SerializeField]
        private Color lightColor = Color.white;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Color)]
        [SerializeField]
        private Color darkColor = Color.white;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Color)]
        [SerializeField]
        private Color hcColor = Color.white;

        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Gradient)]
        [SerializeField]
        private Gradient lightGradient = null;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Gradient)]
        [SerializeField]
        private Gradient darkGradient = null;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Gradient)]
        [SerializeField]
        private Gradient hcGradient = null;

        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Material)]
        [SerializeField]
        private Material lightMaterial = null;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Material)]
        [SerializeField]
        private Material darkMaterial = null;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Material)]
        [SerializeField]
        private Material hcMaterial = null;

        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Texture)]
        [SerializeField]
        private Texture lightTexture = null;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Texture)]
        [SerializeField]
        private Texture darkTexture = null;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Texture)]
        [SerializeField]
        private Texture hcTexture = null;

        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Int)]
        [SerializeField]
        private int lightInt = 0;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Int)]
        [SerializeField]
        private int darkInt = 0;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Int)]
        [SerializeField]
        private int hcInt = 0;

        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Float)]
        [SerializeField]
        private float lightFloat = 0;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Float)]
        [SerializeField]
        private float darkFloat = 0;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Float)]
        [SerializeField]
        private float hcFloat = 0;

        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Vector4)]
        [SerializeField]
        private Vector4 lightVector4 = Vector4.zero;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Vector4)]
        [SerializeField]
        private Vector4 darkVector4 = Vector4.zero;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.Vector4)]
        [SerializeField]
        private Vector4 hcVector4 = Vector4.zero;

        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.TMPGradient)]
        [SerializeField]
        private TMP_ColorGradient lightTmpGradient = null;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.TMPGradient)]
        [SerializeField]
        private TMP_ColorGradient darkTmpGradient = null;
        [ConditionalVisibility(nameof(itemType), ConditionResultType.ShowIfEquals, (int)ThemeItemType.TMPGradient)]
        [SerializeField]
        private TMP_ColorGradient hcTmpGradient = null;
        #endregion

        public bool ThemeRecordValid
        {
            get => themeRecord != null;
        }

        private List<string> availableItemNames = null;
        protected List<string> ItemNameOptions
        {
            get
            {
                if (availableItemNames == null || (ThemeRecordValid && availableItemNames.Count != themeRecord.RecordItems.Count))
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
                    themeRecord.TryGetInfoForId(itemId, out selected, out itemType);
                    name = selected;
                }
                else
                {
                    itemType = ThemeItemType.None;
                }
                return selected;
            }
            set
            {
                if (themeRecord && themeRecord.TryGetInfoForName(value, out int id, out ThemeItemType type))
                {
                    itemId = id;
                    name = value;
                    itemType = type;
                }
                else
                {
                    itemType = ThemeItemType.None;
                }
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

            if (themeRecord.TryGetInfoForId(itemId, out string name, out itemType))
            {
                SelectedItemName = name;
            }
            else
            {
                // insert an empty string at the beginning of the list if no valid option has been selected
                availableItemNames.Insert(0, string.Empty);
            }
        }
    }


    [CreateAssetMenu(fileName = "ThemeValueList", menuName ="MVVM/Theming/Theme Value List")]
    public class ThemeValueList : ScriptableObject
    {
        [SerializeField]
        private List<ThemeItemValue> items = null;

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
