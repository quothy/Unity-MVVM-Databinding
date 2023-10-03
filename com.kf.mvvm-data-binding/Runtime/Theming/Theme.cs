// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [CreateAssetMenu(fileName = "Theme", menuName = "MVVM/Theming/Theme")]
    public class Theme : ScriptableObject
    {
        public List<ThemeValueList> ThemeValueListCollection = null;

        public void Editor_LoadTheme()
        {
            if (ThemeManager.IsInitialized)
            {
                ThemeManager.Instance.LoadTheme(this);
            }
        }
    }
}