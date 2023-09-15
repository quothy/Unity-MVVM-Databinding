using System.Collections;
using System.Collections.Generic;
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