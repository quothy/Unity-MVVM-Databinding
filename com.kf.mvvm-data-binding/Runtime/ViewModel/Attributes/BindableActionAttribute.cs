// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BindableActionAttribute : Attribute
    {
        public int DataItemId = -1;

        public BindableActionAttribute(int dataItemId)
        {
            DataItemId = dataItemId;
        }
    }
}
