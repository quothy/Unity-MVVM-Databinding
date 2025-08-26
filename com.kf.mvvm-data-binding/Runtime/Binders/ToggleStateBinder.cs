using UnityEngine;
using UnityEngine.UI;

namespace MVVMDatabinding
{
    public class ToggleStateBinder : BaseBinder<bool>
    {
        [SerializeField]
        private Toggle toggle = null;

        public override void Bind(GameObject bindingObject)
        {
            base.Bind(bindingObject);

            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnToggleChanged);
            }
        }

        protected override void OnDataUpdated(bool dataValue)
        {
            if (toggle != null)
            {                
                toggle.isOn = dataValue;
            }
        }

        public override void Unbind()
        {
            base.Unbind();
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(OnToggleChanged);
            }
        }

        private void OnToggleChanged(bool value)
        {
            TrySetDataValue(value);
        }
    }
}