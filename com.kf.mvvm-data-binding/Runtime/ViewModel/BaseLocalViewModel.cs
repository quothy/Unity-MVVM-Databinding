using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public abstract class BaseLocalViewModel : BaseViewModel
    {
        public virtual void SetBindingData(int sourceId, int itemId) { }

        public virtual void SetBindingData(int sourceId, int itemId, int index) { }

        public virtual void ClearBindingData() { }
    }
}