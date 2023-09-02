using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    [Serializable]
    public class DataRecordItem
    {
        public int Id = -1;
        public string Name = string.Empty;
    }

    public class DataRecord : ScriptableObject
    {
        public List<DataRecordItem> DataItems = null;
    }
}