using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class DataSourceManager : MonoBehaviour
    {
        private static DataSourceManager _instance = null;

        public static DataSourceManager Instance => _instance;

        private static List<IDataSource> dataSourceList = new List<IDataSource>();

        public static void RegisterDataSource(IDataSource dataSource)
        {
            dataSourceList.Add(dataSource);
        }

        public static void UnregisterDataSource(IDataSource dataSource)
        {
            dataSourceList.Remove(dataSource);
        }

        private void Awake()
        {
            _instance = this;
        }        
    }
}