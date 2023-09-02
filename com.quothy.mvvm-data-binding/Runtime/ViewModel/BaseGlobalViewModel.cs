using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class BaseGlobalViewModel : BaseViewModel
    {
        protected override bool IsGlobalSource => true;
    }
}