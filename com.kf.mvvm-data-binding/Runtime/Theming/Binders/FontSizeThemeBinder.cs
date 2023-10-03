using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    public class FontSizeThemeBinder : BaseThemeBinder<int>
    {
        [SerializeField]
        private List<TMP_Text> targets = null;

        protected override ThemeItemType ThemeItemType => ThemeItemType.Int;

        protected override void OnDataUpdated(int dataValue)
        {
            foreach (TMP_Text text in targets)
            {
                text.fontSize = dataValue;
            }
        }
    }
}