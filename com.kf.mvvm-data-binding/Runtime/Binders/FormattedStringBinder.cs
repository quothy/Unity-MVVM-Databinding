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
            if (dataSource.TryGetItem<int>(itemId, out int intValue))
            {
                if (targetText != null)
                {
                    targetText.text = string.Format(format, intValue);
                }
            }
            else if (dataSource.TryGetItem<float>(itemId, out float floatValue))
            {
                if (targetText != null)
                {
                    targetText.text = string.Format(format, floatValue);
                }
            }
            else if (dataSource.TryGetItem<string>(itemId, out string stringValue))
            {
                if (targetText != null)
                {
                    targetText.text = string.Format(format, stringValue);
                }
            }
            else if (dataSource.TryGetItem<Vector2Int>(itemId, out Vector2Int vec2IntValue))
            {
                if (targetText != null)
                {
                    targetText.text = string.Format(format, vec2IntValue.x, vec2IntValue.y);
                }                
            }
            else if (dataSource.TryGetItem<Vector2>(itemId, out Vector2 vec2Value))
            {
                if (targetText != null)
                {
                    targetText.text = string.Format(format, vec2Value.x, vec2Value.y);
                }                
            }
        }
    }
}
