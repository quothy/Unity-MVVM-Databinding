// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding.Theming
{
    public class SpriteImageThemeBinder : BaseThemeBinder<Sprite>
    {
        [SerializeField]
        private List<Image> targets = null; 

        protected override ThemeItemType ThemeItemType => ThemeItemType.Sprite;

        protected override void OnDataUpdated(Sprite dataValue)
        {
            if (targets != null)
            {
                foreach (Image target in targets)
                {
                    target.sprite = dataValue;
                }
            }
        }
    }
}
