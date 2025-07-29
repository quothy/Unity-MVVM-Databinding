// Copyright (c) 2025 Katie Fremont
// Licensed under the MIT license

using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding
{
    public class ColorSwapBinder : BaseBinder<bool>
    {
        [SerializeField]
        private Graphic target;

        [SerializeField]
        private Color activeColor = Color.white;
        
        [SerializeField]
        private Color inactiveColor = Color.white;

        protected override void OnDataUpdated(bool dataValue)
        {
            if (target != null)
            {
                target.color = dataValue ? activeColor : inactiveColor;
            }
        }
    }
}