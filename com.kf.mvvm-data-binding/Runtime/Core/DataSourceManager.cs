// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class DataSourceManager : MonoBehaviour
    {
        private class PendingSubscription
        {
            public int DataItemId;
            public DataItemUpdate OnUpdate;
        }


        private static DataSourceManager _instance = null;

        public static DataSourceManager Instance => _instance;

        private static Dictionary<int, IDataSource> dataSourceLookup = new Dictionary<int, IDataSource>();
        private static Dictionary<int, List<PendingSubscription>> pendingSubscriptons = new Dictionary<int, List<PendingSubscription>>();

        public static void RegisterDataSource(IDataSource dataSource)
        {
            if (!dataSourceLookup.TryGetValue(dataSource.Id, out IDataSource existing))
            {
                dataSourceLookup[dataSource.Id] = dataSource;

                if (pendingSubscriptons.TryGetValue(dataSource.Id, out List<PendingSubscription> pendingList))
                {
                    foreach (PendingSubscription pending in pendingList)
                    {
                        dataSource.SubscribeToItem(pending.DataItemId, pending.OnUpdate);
                    }    

                    pendingSubscriptons.Remove(dataSource.Id);
                }

                //Debug.Log($"[DataSourceManager] Registered source {dataSource.Name} to id {dataSource.Id}");
            }
            else
            {
                Debug.LogWarning($"[DataSourceManager] A data source with id {dataSource.Id} (name = {dataSource.Name} is already registered!");
            }
        }

        public static void UnregisterDataSource(IDataSource dataSource)
        {
            if (dataSourceLookup.TryGetValue(dataSource.Id, out IDataSource existing))
            {
                dataSourceLookup.Remove(dataSource.Id);
            }
        }

        public static void SubscribeToItem(int sourceId, int itemId, DataItemUpdate onUpdate)
        {
            if (dataSourceLookup.TryGetValue(sourceId, out IDataSource dataSource))
            {
                dataSource.SubscribeToItem(itemId, onUpdate);
            }
            else
            {
                //Debug.LogWarning($"[DataSourceManager] Cannot subscribe to item {itemId} because data source {sourceId} is not registered yet!");

                if (!pendingSubscriptons.TryGetValue(sourceId, out List<PendingSubscription> subscriptions))
                {
                    pendingSubscriptons[sourceId] = new List<PendingSubscription>();
                }

                pendingSubscriptons[sourceId].Add(new PendingSubscription() { DataItemId = itemId, OnUpdate = onUpdate });
            }
        }

        public static void UnsubscribeFromItem(int sourceId, int itemId, DataItemUpdate onUpdate)
        {
            if (dataSourceLookup.TryGetValue(sourceId, out IDataSource dataSource))
            {
                dataSource.UnsubscribeFromItem(itemId, onUpdate);
            }
        }

        public static bool TryGetDataSource(int sourceId, out IDataSource dataSource)
        {
            return dataSourceLookup.TryGetValue(sourceId, out dataSource);
        }

        private void Start()
        {
            _instance = this;
        }        
    }
}