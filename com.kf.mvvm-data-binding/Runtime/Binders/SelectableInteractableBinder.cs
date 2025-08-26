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

        [SerializeField]
        private bool invert = false;

        protected override void OnDataUpdated(bool value)
        {
            if (targetSelectable != null)
            {
                bool active = invert ? !value : value;
                targetSelectable.interactable = active;
                targetSelectable.enabled = false;
                targetSelectable.enabled = true;
            }
        }
    }
}
