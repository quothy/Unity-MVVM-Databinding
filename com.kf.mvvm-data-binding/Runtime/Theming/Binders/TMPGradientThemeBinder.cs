using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    public class TMPGradientThemeBinder : BaseThemeBinder<TMP_ColorGradient>
    {
        [SerializeField]
        private List<TMP_Text> targets = null;

        protected override ThemeItemType ThemeItemType => ThemeItemType.TMPGradient;

        protected override void OnDataUpdated(TMP_ColorGradient dataValue)
        {
            foreach (TMP_Text text in targets)
            {
                if (text != null)
                {
                    text.colorGradientPreset = dataValue;
                }
            }
        }
    }
}