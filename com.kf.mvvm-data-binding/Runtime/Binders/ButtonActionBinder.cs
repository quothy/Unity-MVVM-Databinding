// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding
{
    public class ButtonActionBinder : BaseBinder<Action>
    {
        //protected string name = "Button Action Binder";

        [ConditionalVisibility(nameof(DataRecordValid), ConditionResultType.ShowIfEquals)]
        [ConditionalEnable("", ConditionalEnableAttribute.ConditionalEnableType.Never)]
        [SerializeField]
        private BindDirection bindDirection = BindDirection.OneWayToSource;

        [SerializeField]
        private Button button = null;

        //public override string Name { get => name; }

        public override void Bind(GameObject bindingObject)
        {
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }

            base.Bind(bindingObject);
        }

        public override void Unbind()
        {
            base.Unbind();

            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }

        protected override void OnDataUpdated(Action dataValue) { }

        protected void OnButtonClick()
        {
            if (TryGetDataSource(out IDataSource dataSource) && dataSource.TryGetItem<Action>(itemId, out Action buttonAction))
            {
                buttonAction?.Invoke();
            }
        }
    }
}