// Copyright (c) 2026 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding.Theming
{
    public class EnabledStateThemeBinder : BaseThemeBinder<bool>
    {
        [SerializeField]
        private List<GameObject> targets = null; 

        [SerializeField]
        private bool invert = false;

        protected override ThemeItemType ThemeItemType => ThemeItemType.Bool;

        protected override void OnDataUpdated(Bool dataValue)
        {
            if (targets != null)
            {
                foreach (GameObject target in targets)
                {
                    target.SetActive(invert ? !dataValue : dataValue);
                }
            }
        }
    }
}
