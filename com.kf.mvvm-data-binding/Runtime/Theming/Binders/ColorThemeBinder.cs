// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding.Theming
{
    public class ColorThemeBinder : BaseThemeBinder<Color>
    {
        [SerializeField]
        private Graphic target = null; 

        protected override ThemeItemType ThemeItemType => ThemeItemType.Color;

        protected override void OnDataUpdated(Color dataValue)
        {
            if (target != null)
            {
                target.color = dataValue;
            }
        }
    }
}
