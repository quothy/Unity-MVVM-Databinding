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
        void Subscribe();
        void Unsubscribe();

        void OnDataItemUpdate(IDataSource dataSource, int itemId);
    }
}