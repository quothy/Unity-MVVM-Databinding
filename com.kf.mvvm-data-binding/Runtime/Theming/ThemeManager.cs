// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    public class ThemeManager : MonoBehaviour
    {
        private static readonly string themingSourcePrefix = "Theming";
        private static ThemeManager instance = null;
        public static bool IsInitialized => instance != null;
        public static ThemeManager Instance => instance;

        public static event Action ThemeChanged = null;

        public static int GetThemeSourceId(string recordName)
        {
            return Animator.StringToHash(themingSourcePrefix + "-" + recordName);
        }

        [SerializeField]
        private Theme defaultTheme = null;

        private Dictionary<int, ThemeDataSource> themeDataSourceLookup = new Dictionary<int, ThemeDataSource>();

        public ThemeVariant ActiveVariant { get; private set; } = ThemeVariant.Light;


        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (defaultTheme != null)
            {
                LoadTheme(defaultTheme);
            }
        }

        private void OnDestroy()
        {
            instance = null;
        }

        public void LoadTheme(Theme theme)
        {
            foreach (ThemeStyle style in theme.ThemeStyleList)
            {
                foreach (ThemeStyleValue styleValue in style.Values)
                {
                    int sourceId = GetThemeSourceId(style.StyleName);
                    if (!themeDataSourceLookup.TryGetValue(sourceId, out ThemeDataSource themeDataSource))
                    {
                        themeDataSource = new ThemeDataSource();
                        themeDataSource.Initialize(style.StyleName, false);
                        themeDataSourceLookup[sourceId] = themeDataSource;
                        themeDataSource.SetThemeVariant(ActiveVariant);
                    }

                    if (!themeDataSource.TryGetThemeItem(styleValue.Id, out ThemeItem themeItem))
                    {
                        // TODO: pool these for performance? It'll at least make sure we allocate most of the memory up front
                        themeItem = new ThemeItem();
                        themeItem.Initialize(styleValue.Id, styleValue.Name);
                        themeDataSource.AddItem(themeItem);
                        themeItem.SetThemeVariant(ActiveVariant);
                    }

                    themeItem.SetThemeItemValue(styleValue);

                }
            }

            //foreach (ThemeValueList valueList in theme.ThemeValueListCollection)
            //{
            //    foreach (ThemeItemValue themeItemValue in valueList.Items)
            //    {
            //        int sourceId = GetThemeSourceId(themeItemValue.RecordName);
            //        if (!themeDataSourceLookup.TryGetValue(sourceId, out ThemeDataSource themeDataSource))
            //        {
            //            themeDataSource = new ThemeDataSource();
            //            themeDataSource.Initialize(themeItemValue.RecordName, false);
            //            themeDataSourceLookup[sourceId] = themeDataSource;
            //            themeDataSource.SetThemeVariant(ActiveVariant);
            //        }

            //        if (!themeDataSource.TryGetThemeItem(themeItemValue.Id, out ThemeItem themeItem))
            //        {
            //            // TODO: pool these for performance? It'll at least make sure we allocate most of the memory up front
            //            themeItem = new ThemeItem();
            //            themeItem.Initialize(themeItemValue.Id, themeItemValue.Name);
            //            themeDataSource.AddItem(themeItem);
            //            themeItem.SetThemeVariant(ActiveVariant);
            //        }

            //        themeItem.SetThemeItemValue(themeItemValue);
            //    }
            //}
        }

        public void ChangeThemeVariant(ThemeVariant variant)
        {
            if (ActiveVariant == variant)
            {
                return;
            }

            ActiveVariant = variant;
            foreach (var kvp in themeDataSourceLookup)
            {
                kvp.Value.SetThemeVariant(variant);
            }
        }
    }
}