using UnityEngine;
using TMPro;
using MVVMDatabinding;

namespace MVVMDatabinding
{
    // Binds an int, float, or string property in the ViewModel to a TMP_Text, using a format string
    public class FormattedStringBinder : BaseBinder
    {
        [SerializeField]
        private TMP_Text targetText = null;

        [SerializeField]
        private string format = "{0}";

        public override void OnDataItemUpdate(IDataSource dataSource, int itemId)
        {
            object value = null;
            if (dataSource.TryGetItem<int>(itemId, out int intValue))
                value = intValue;
            else if (dataSource.TryGetItem<float>(itemId, out float floatValue))
                value = floatValue;
            else if (dataSource.TryGetItem<string>(itemId, out string stringValue))
                value = stringValue;

            if (value != null && targetText != null)
            {
                targetText.text = string.Format(format, value);
            }
        }
    }
}
