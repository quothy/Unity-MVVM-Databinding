using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public enum DataSourceIdResolutionType
    {
        None,
        ManuallySetInstance,
        GetComponentInParent,
    }

    [Serializable]
    public abstract class BaseBinder : IBinder
    {
        [SerializeField]
        private DataRecord dataRecord = null;

        [SerializeField]
        private int itemId = -1;

        /// <summary>
        /// We're going to want to display strings for the items on the Inspector UI
        /// </summary>
        private List<string> availableItemNames = null;
        public string ItemName { get; private set; }

        public void Subscribe()
        {
            DataSourceManager.SubscribeToItem(dataRecord.SourceId, itemId, OnDataItemUpdate);
        }

        public void Unsubscribe()
        {
            DataSourceManager.UnsubscribeFromItem(dataRecord.SourceId, itemId, OnDataItemUpdate);
        }

        public abstract void OnDataItemUpdate(IDataSource dataSource, int itemId);
    }
}