// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding
{
    public class TextureBinder : BaseBinder<Texture>
    {
        [SerializeField]
        private RawImage target = null;

        protected override void OnDataUpdated(Texture dataValue)
        {
            if (target != null)
            {
                target.texture = dataValue;
            }
        }
    }
}