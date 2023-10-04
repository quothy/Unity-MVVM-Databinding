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
            EnsureCacheInitialized();

            // make sure dataItemType implements IDataItem before adding to the cache
            if (!typeof(IDataItem).IsAssignableFrom(dataItemType))
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
            EnsureCacheInitialized();
            dataItemType = null;
            if (typeof(IList).IsAssignableFrom(underlyingDataType))
            {
                return dataItemTypeCache.TryGetValue(typeof(IList), out dataItemType);
            }
            return dataItemTypeCache.TryGetValue(underlyingDataType, out dataItemType);
        }

        private static void EnsureCacheInitialized()
        {
            if (dataItemTypeCache == null)
            {
                dataItemTypeCache = new Dictionary<Type, Type>();
                RegisterBuiltInTypes();
            }
        }

        private static void RegisterBuiltInTypes()
        {
            dataItemTypeCache[typeof(int)] = typeof(DataItemInt);
            dataItemTypeCache[typeof(long)] = typeof(DataItemLong);
            dataItemTypeCache[typeof(float)] = typeof(DataItemFloat);
            dataItemTypeCache[typeof(bool)] = typeof(DataItemBool);
            dataItemTypeCache[typeof(string)] = typeof(DataItemString);
            dataItemTypeCache[typeof(Color)] = typeof(DataItemColor);
            dataItemTypeCache[typeof(Material)] = typeof(DataItemMaterial);
            dataItemTypeCache[typeof(Sprite)] = typeof(DataItemSprite);
            dataItemTypeCache[typeof(Texture)] = typeof(DataItemTexture);
            dataItemTypeCache[typeof(Vector2)] = typeof(DataItemVector2);
            dataItemTypeCache[typeof(Vector3)] = typeof(DataItemVector3);
            dataItemTypeCache[typeof(Vector4)] = typeof(DataItemVector4);

            dataItemTypeCache[typeof(IList)] = typeof(DataItemList);

            dataItemTypeCache[typeof(Action)] = typeof(DataItemAction);
        }
    }
}