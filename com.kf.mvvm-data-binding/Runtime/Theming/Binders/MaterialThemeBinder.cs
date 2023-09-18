// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding.Theming
{
    public class MaterialThemeBinder : BaseThemeBinder<Material>
    {
        [SerializeField]
        private Graphic target = null;

        protected override ThemeItemType ThemeItemType => ThemeItemType.Material;

        protected override void OnDataUpdated(Material dataValue)
        {
            if (target != null)
            {
                target.material = dataValue;
            }
        }
    }
}
