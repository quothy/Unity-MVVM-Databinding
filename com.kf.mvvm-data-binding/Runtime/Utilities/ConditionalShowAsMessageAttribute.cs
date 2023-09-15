// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEngine;

namespace MVVMDatabinding
{
    public class ConditionalShowAsMessageAttribute : PropertyAttribute
    {
        public enum MessageType
        {
            Info,
            Warning,
            Error,
        }

        public string ComparisonPropertyName { get; set; }
        public ConditionComparisonType ComparisonType { get; set; }
        public ConditionResultType Condition { get; set; }
        public MessageType Type { get; set; }

        public int TargetIntValue { get; set; }

        public ConditionalShowAsMessageAttribute(string condition, ConditionResultType resultType, MessageType messageType)
        {
            ComparisonType = ConditionComparisonType.Bool;
            ComparisonPropertyName = condition;
            Condition = resultType;
            Type = messageType;
        }

        public ConditionalShowAsMessageAttribute(string condition, ConditionResultType resultType, int targetEnumValue, MessageType messageType)
        {
            ComparisonType = ConditionComparisonType.Enum;
            ComparisonPropertyName = condition;
            Condition = resultType;
            Type = messageType;
            TargetIntValue = targetEnumValue;
        }
    }
}