using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ConditionalEnableAttribute : PropertyAttribute
    {
        public enum ConditionalEnableType
        {
            EnableIfTrue,
            DisableIfTrue,
            Never
        }

        public string ConditionPropertyName { get; private set; }
        public ConditionalEnableType Condition { get; private set; }

        public ConditionalEnableAttribute(string conditionPropertyName, ConditionalEnableType conditionType)
        {
            ConditionPropertyName = conditionPropertyName;
            Condition = conditionType;
        }
    }
}