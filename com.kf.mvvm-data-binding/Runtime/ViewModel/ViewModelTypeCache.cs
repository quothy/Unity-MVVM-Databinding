using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public static class ViewModelTypeCache
    {
        private static Dictionary<string, Type> viewModelTypeCache = null;

        public static void RegisterViewModelType(string viewModelTypeString, Type viewModelType)
        {
            EnsureCacheInitialized();

            // make sure dataItemType implements IDataItem before adding to the cache
            if (!typeof(BaseViewModel).IsAssignableFrom(viewModelType))//.IsAssignableFrom(typeof(BaseViewModel)))
            {
                Debug.LogErrorFormat("[DataItemTypeCache] Cannot add {0} as the type for {1} because it does not inherit from ViewModelBase!", viewModelType, viewModelTypeString);
                return;
            }

            if (!viewModelTypeCache.TryGetValue(viewModelTypeString, out Type existing))
            {
                viewModelTypeCache[viewModelTypeString] = viewModelType;
            }
        }

        public static bool TryGetViewModelType(string typeName, out Type viewModelType)
        {
            EnsureCacheInitialized();
            viewModelType = null;
            return viewModelTypeCache.TryGetValue(typeName, out viewModelType);
        }

        private static void EnsureCacheInitialized()
        {
            if (viewModelTypeCache == null)
            {
                viewModelTypeCache = new Dictionary<string, Type>();
            }
        }
    }
}