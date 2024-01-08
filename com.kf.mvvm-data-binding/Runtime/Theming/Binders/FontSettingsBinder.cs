using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    public class FontSettingsBinder : BaseThemeBinder<ThemeFontSettings>
    {
        [SerializeField]
        private TMPro.TMP_Text text = null;

        protected override ThemeItemType ThemeItemType => ThemeItemType.FontSettings;

        protected override void OnDataUpdated(ThemeFontSettings dataValue)
        {
            if (dataValue != null && text != null)
            {
                text.font = dataValue.FontAsset;
                text.enableAutoSizing = dataValue.AutosizeFont;
                if (dataValue.AutosizeFont)
                {
                    text.fontSizeMin = dataValue.MinSize;
                    text.fontSizeMax = dataValue.MaxSize;
                }
                else
                {
                    text.fontSize = dataValue.FontSize;
                }
                text.fontStyle = dataValue.FontStyle;
                text.lineSpacing = dataValue.LineHeight;
                text.characterSpacing = dataValue.CharacterSpacing;
            }
        }
    }
}