// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class VisibilityBinder : BaseBinder<bool>
    {
        [SerializeField]
        private GameObject targetObject = null;

        [SerializeField]
        private List<GameObject> targetList = null;

        [SerializeField]
        private bool invertVisibility = false;

        protected override void OnDataUpdated(bool dataValue)
        {
            bool visible = invertVisibility ? !dataValue : dataValue;
            if (targetObject != null)
            {
                targetObject.SetActive(visible);
            }
            if (targetList != null)
            {
                foreach (var target in targetList)
                {
                    if (target != null)
                    {
                        target.SetActive(visible);
                    }
                }
            }
        }
    }
}