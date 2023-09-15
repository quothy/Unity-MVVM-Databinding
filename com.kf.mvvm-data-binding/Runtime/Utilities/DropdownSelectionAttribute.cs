// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DropdownSelectionAttribute : PropertyAttribute
    {
        public string OptionsSourcePropertyName { get; private set; }
        public string SelectedOptionPropertyName { get; private set; }
        public DropdownSelectionAttribute(string optionsSourcePropertyName, string selectedOptionPropertyName)
        {
            OptionsSourcePropertyName = optionsSourcePropertyName;
            SelectedOptionPropertyName = selectedOptionPropertyName;
        }
    }
}