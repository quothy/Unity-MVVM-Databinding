using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ConditionalVisibilityAttribute : PropertyAttribute
    {
        public enum ConditionalVisibilityType
        {
            ShowIfTrue,
            HideIfTrue,
            Never
        }

        public string ConditionPropertyName { get; private set; }
        public ConditionalVisibilityType Condition { get; private set; }

        public ConditionalVisibilityAttribute(string conditionPropertyName, ConditionalVisibilityType conditionType)
        {
            ConditionPropertyName = conditionPropertyName;
            Condition = conditionType;
        }
    }
}