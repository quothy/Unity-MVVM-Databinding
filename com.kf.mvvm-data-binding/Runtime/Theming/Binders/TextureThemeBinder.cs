// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding.Theming
{
    public class TextureThemeBinder : BaseThemeBinder<Texture>
    {
        [SerializeField]
        private List<RawImage> targets = null;

        protected override ThemeItemType ThemeItemType => ThemeItemType.Texture;

        protected override void OnDataUpdated(Texture dataValue)
        {
            if (targets != null)
            {
                foreach (RawImage target in targets)
                {
                    target.texture = dataValue;
                }
            }
        }
    }
}
