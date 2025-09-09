// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public enum ConditionComparisonType
    {
        Bool,
        Enum,
    }

    public enum ConditionResultType
    {
        ShowIfEquals,
        ShowIfNotEquals,
        Never,
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ConditionalVisibilityAttribute : PropertyAttribute
    {
        public ConditionComparisonType ComparisonType { get; set; }

        public string ConditionPropertyName { get; private set; }
        public ConditionResultType Condition { get; private set; }

        public int TargetIntValue { get; private set; }

        public ConditionalVisibilityAttribute(string conditionPropertyName)
        {
            ComparisonType = ConditionComparisonType.Bool;
            ConditionPropertyName = conditionPropertyName;
            Condition = ConditionResultType.ShowIfEquals;
        }

        public ConditionalVisibilityAttribute(string conditionPropertyName, ConditionResultType conditionType)
        {
            ComparisonType = ConditionComparisonType.Bool;
            ConditionPropertyName = conditionPropertyName;
            Condition = conditionType;
        }

        public ConditionalVisibilityAttribute(string conditionPropertyName, ConditionResultType conditionType, int targetEnumValue)
        {
            ComparisonType = ConditionComparisonType.Enum;
            ConditionPropertyName = conditionPropertyName;
            Condition = conditionType;
            TargetIntValue = targetEnumValue;
        }
    }
}