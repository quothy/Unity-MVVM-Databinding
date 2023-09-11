using System.Collections;
using System.Collections.Generic;
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

        void Bind(GameObject bindingObject);
        void Unbind();

        void OnDataItemUpdate(IDataSource dataSource, int itemId);
    }
}