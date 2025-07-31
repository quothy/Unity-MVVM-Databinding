using UnityEngine;
using UnityEngine.UI;
using MVVMDatabinding;

namespace MVVMDatabinding
{
    // Binds a bool property in the ViewModel to the Interactable state of a UnityEngine.UI.Selectable
    public class SelectableInteractableBinder : BaseBinder<bool>
    {
        [SerializeField]
        private Selectable targetSelectable = null;

        protected override void OnDataUpdated(bool value)
        {
            if (targetSelectable != null)
            {
                targetSelectable.interactable = value;
                targetSelectable.enabled = false;
                targetSelectable.enabled = true;
            }
        }
    }
}
