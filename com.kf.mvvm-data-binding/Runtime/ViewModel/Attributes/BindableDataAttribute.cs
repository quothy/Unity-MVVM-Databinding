using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BindableDataAttribute : Attribute
    {
        public int DataItemId = -1;

        public BindableDataAttribute(int dataItemId)
        {
            DataItemId = dataItemId;
        }
    }
}