// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using TMPro;
using UnityEngine;

namespace MVVMDatabinding
{
    [Serializable]
    public class TextBinder : BaseBinder<string>
    {
        [SerializeField]
        private BindDirection bindDirection = BindDirection.OneWay;

        [ConditionalVisibility(nameof(bindDirection), ConditionResultType.ShowIfEquals, (int)BindDirection.OneWay)]
        [SerializeField]
        private TextMeshProUGUI text = null;

        [ConditionalVisibility(nameof(bindDirection), ConditionResultType.ShowIfNotEquals, (int)BindDirection.OneWay)]
        [SerializeField]
        private TMP_InputField textInput = null;

        public override void Bind(GameObject bindingObject)
        {
            base.Bind(bindingObject);

            if (bindDirection != BindDirection.OneWay && textInput != null)
            {
                textInput.onValueChanged.AddListener(OnTextEdited);
            }
        }

        public override void Unbind()
        {
            base.Unbind();

            if (bindDirection != BindDirection.OneWay && textInput != null)
            {
                textInput.onValueChanged.RemoveListener(OnTextEdited);
            }
        }

        protected override void OnDataUpdated(string dataValue)
        {
            if (text != null && bindDirection == BindDirection.OneWay)
            {
                text.text = dataValue;
            }
            else if (textInput != null && bindDirection == BindDirection.TwoWay)
            {
                textInput.text = dataValue;
            }
        }

        private void OnTextEdited(string textData)
        {
            if (TryGetDataSource(out IDataSource source))
            {
                source.TrySetItem<string>(itemId, textData);
            }
        }
    }
}