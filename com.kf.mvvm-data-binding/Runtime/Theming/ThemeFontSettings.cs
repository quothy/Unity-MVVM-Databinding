using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [Serializable]
    public class ThemeFontSettings
    {
        public TMP_FontAsset FontAsset = null;
        public bool AutosizeFont = false;
        [ConditionalVisibility(nameof(AutosizeFont), ConditionResultType.ShowIfNotEquals)]
        public int FontSize = 16;
        [ConditionalVisibility(nameof(AutosizeFont), ConditionResultType.ShowIfEquals)]
        public int MinSize = 12;
        [ConditionalVisibility(nameof(AutosizeFont), ConditionResultType.ShowIfEquals)]
        public int MaxSize = 12;

        public FontStyles FontStyle = FontStyles.Normal;
        public int LineHeight = 0;
        public int CharacterSpacing = 0;
    }
}