using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public static class DataItemTypeCache
    {
        private static Dictionary<Type, Type> dataItemTypeCache = null;

        public static void RegisterDataItemType(Type underlyingDataType, Type dataItemType)
        {
            if (dataItemTypeCache == null)
            {
                dataItemTypeCache = new Dictionary<Type, Type>();
                RegisterBuiltInTypes();
            }

            // make sure dataItemType implements IDataItem before adding to the cache
            if (!dataItemType.IsAssignableFrom(typeof(IDataItem)))
            {
                Debug.LogErrorFormat("[DataItemTypeCache] Cannot add {0} as the type for {1} because it does not implement IDataItem!", dataItemType, underlyingDataType);
                return;
            }

            if (!dataItemTypeCache.TryGetValue(underlyingDataType, out Type existing))
            {
                dataItemTypeCache[underlyingDataType] = dataItemType;
            }
        }

        public static bool TryGetDataItemType(Type underlyingDataType, out Type dataItemType)
        {
            dataItemType = null;
            return dataItemTypeCache.TryGetValue(underlyingDataType, out dataItemType);
        }

        private static void RegisterBuiltInTypes()
        {
            dataItemTypeCache[typeof(int)] = typeof(DataItemInt);
            dataItemTypeCache[typeof(float)] = typeof(DataItemFloat);
            dataItemTypeCache[typeof(bool)] = typeof(DataItemBool);
            dataItemTypeCache[typeof(string)] = typeof(DataItemString);
        }
    }
}