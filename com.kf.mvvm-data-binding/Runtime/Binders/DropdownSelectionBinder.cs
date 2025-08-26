using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MVVMDatabinding
{
    public class DropdownSelectionBinder : BaseBinder<DataList>
    {
        [SerializeField]
        private TMP_Dropdown dropdown = null;

        private DataList boundList = null;

        public override void Bind(GameObject bindingObject)
        {
            base.Bind(bindingObject);
            if (dropdown != null)
            {
                dropdown.onValueChanged.AddListener(OnIndexChanged);
            }
        }

        private void OnIndexChanged(int index)
        {
            if (boundList != null)
            {
                boundList.SelectedIndex = index;
            }
        }

        public override void Unbind()
        {
            base.Unbind();
            
            if (dropdown != null)
            {
                dropdown.onValueChanged.RemoveListener(OnIndexChanged);
            }
        }

        protected override void OnDataUpdated(DataList dataValue)
        {
            boundList = dataValue;
        }
    }
}