// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEngine;

namespace MVVMDatabinding
{
    public enum BindDirection
    {
        OneWay,
        OneWayToSource,
        TwoWay,
    }

    public interface IBinder
    {
        string Name { get; }
        bool DataRecordValid { get; }

        void OnEnable();
        void OnDisable();

        void Bind(GameObject bindingObject);
        void Unbind();

        void OnDataItemUpdate(IDataSource dataSource, int itemId);
#if UNITY_EDITOR
        void OnValidate(GameObject go);
#endif
    }
}