// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding
{
    public class ColorBinder : BaseBinder<Color>
    {
        [SerializeField]
        private Graphic target;

        protected override void OnDataUpdated(Color dataValue)
        {
            if (target != null)
            {
                target.color = dataValue;
            }
        }
    }
}