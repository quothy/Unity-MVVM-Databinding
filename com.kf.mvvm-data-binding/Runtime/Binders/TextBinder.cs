using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UIElements;

namespace MVVMDatabinding
{
    [Serializable]
    public class TextBinder : BaseBinder<string>
    {
        //protected string name = "Text Binder";

        [SerializeField]
        private BindDirection bindDirection = BindDirection.OneWay;

        [ConditionalVisibility(nameof(bindDirection), ConditionResultType.ShowIfEquals, (int)BindDirection.OneWay)]
        [SerializeField]
        private TextMeshProUGUI text = null;

        //public override string Name { get => name; }

        protected override void OnDataUpdated(string dataValue)
        {
            if (text != null)
            {
                text.text = dataValue;
            }
        }
    }
}