// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public abstract class BaseGlobalViewModel : BaseViewModel
    {
        public override bool IsGlobalSource => true;
    }
}