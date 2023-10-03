// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding.Theming
{
    public class MaterialThemeBinder : BaseThemeBinder<Material>
    {
        [SerializeField]
        private List<Graphic> targets = null;

        protected override ThemeItemType ThemeItemType => ThemeItemType.Material;

        protected override void OnDataUpdated(Material dataValue)
        {
            if (targets != null)
            {
                foreach (Graphic target in targets)
                {
                    target.material = dataValue;
                }
            }
        }
    }
}
