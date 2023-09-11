using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class StringSwapBinder : BaseBinder<bool>
    {
        [SerializeField]
        private TMPro.TMP_Text text = null;

        [SerializeField]
        private string trueString = string.Empty;

        [SerializeField]
        private string falseString = string.Empty;

        protected override void OnDataUpdated(bool dataValue)
        {
            if (text != null)
            {
                text.text = dataValue ? trueString : falseString;
            }
        }
    }
}