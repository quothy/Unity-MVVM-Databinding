// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEngine;

namespace MVVMDatabinding
{
    public class VisibilityBinder : BaseBinder<bool>
    {
        [SerializeField]
        private GameObject targetObject = null;

        [SerializeField]
        private bool invertVisibility = false;

        protected override void OnDataUpdated(bool dataValue)
        {
            bool visible = invertVisibility ? !dataValue : dataValue;
            if (targetObject != null)
            {
                targetObject.SetActive(visible);
            }
        }
    }
}