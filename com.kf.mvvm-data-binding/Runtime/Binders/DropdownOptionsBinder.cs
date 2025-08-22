using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace MVVMDatabinding
{
    public class DropdownOptionsBinder : BaseBinder<DataList>
    {
        [SerializeField]
        private TMP_Dropdown dropdown = null;

        private List<string> underlyingList = new();

        protected override void OnDataUpdated(DataList dataValue)
        {
            if (dropdown != null)
            {
                dropdown.ClearOptions();
                underlyingList.Clear();
                foreach (string item in dataValue as DataList<string>)
                {
                    underlyingList.Add(item);
                }
                dropdown.AddOptions(underlyingList);
            }
        }
    }
}