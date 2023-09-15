// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding
{
    public class SpriteSwapBinder : BaseBinder<bool>
    {
        [SerializeField]
        private Image target = null;

        [SerializeField]
        private Sprite trueSprite = null;

        [SerializeField]
        private Sprite falseSprite = null;

        protected override void OnDataUpdated(bool dataValue)
        {
            if (target != null)
            {
                target.sprite = dataValue ? trueSprite : falseSprite;
            }
        }
    }
}