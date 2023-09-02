using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class DataSourceManager : MonoBehaviour
    {
        private static DataSourceManager _instance = null;

        public static DataSourceManager Instance => _instance;

        private static Dictionary<int, IDataSource> dataSourceLookup = new Dictionary<int, IDataSource>();

        public static void RegisterDataSource(IDataSource dataSource)
        {
            if (!dataSourceLookup.TryGetValue(dataSource.Id, out IDataSource existing))
            {
                dataSourceLookup[dataSource.Id] = dataSource;
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
                // TODO: Figure out how to handle this case
                Debug.LogWarning($"[DataSourceManager] Cannot subscribe to item because data source {sourceId} is not registered yet!");
            }
        }

        public static void UnsubscribeFromItem(int sourceId, int itemId, DataItemUpdate onUpdate)
        {
            if (dataSourceLookup.TryGetValue(sourceId, out IDataSource dataSource))
            {
                dataSource.UnsubscribeFromItem(itemId, onUpdate);
            }
        }

        private void Awake()
        {
            _instance = this;
        }        
    }
}