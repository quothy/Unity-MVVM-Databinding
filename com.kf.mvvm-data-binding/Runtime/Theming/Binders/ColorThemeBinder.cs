// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding.Theming
{
    public class ColorThemeBinder : BaseThemeBinder<Color>
    {
        [SerializeField]
        private List<Graphic> targets = null; 

        protected override ThemeItemType ThemeItemType => ThemeItemType.Color;

        protected override void OnDataUpdated(Color dataValue)
        {
            if (targets != null)
            {
                foreach (Graphic target in targets)
                {
                    target.color = dataValue;
                }
            }
        }
    }
}
