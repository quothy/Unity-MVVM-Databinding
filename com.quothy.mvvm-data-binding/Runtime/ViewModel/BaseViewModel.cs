using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.IO;
using UnityEditor;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace MVVMDatabinding
{
    public abstract class BaseViewModel : MonoBehaviour
    {
        [SerializeReference]
        private List<IDataItem> dataItemList = null;

        private ViewModelDataSource dataSource = null;

        private string recordDir = "DataRecord";
        protected virtual string RecordPath
        {
            get
            {
                return "Assets/" + recordDir;
            }
        }

        /// <summary>
        /// A ViewModel is considered to be a global/singleton source if there
        /// is only ever expected to be one of them in existence at a time.
        /// </summary>
        public virtual bool IsGlobalSource => false;

        protected virtual void Awake()
        {
            InitializeData();
        }


        public void InitializeData()
        {
            dataSource = new ViewModelDataSource();
            dataSource.InitializeFromViewModel(this);
            dataSource.LoadDataItems(dataItemList);
        }

        protected void OnPropertyChanged([CallerMemberName]string name = "")
        {
            dataSource.OnPropertyChanged(name);
        }

        [ContextMenu("Update Record")]
        public void UpdateRecord()
        {
#if UNITY_EDITOR
            if (dataItemList == null)
            {
                dataItemList = new List<IDataItem>();
            }

            List<int> updatedIds = new List<int>();

            Type vmType = this.GetType();

            // get a list of properties and look for ones with the BindableData attribute
            PropertyInfo[] propInfoList = vmType.GetProperties();
            List<Attribute> attributes = new List<Attribute>();

            foreach (PropertyInfo info in propInfoList)
            {
                attributes.AddRange(info.GetCustomAttributes());
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is BindableDataAttribute bindableAttribute)
                    {
                        // found a data item! add it to the list so we can serialize it
                        if (updatedIds.Contains(bindableAttribute.DataItemId))
                        {
                            Debug.LogError($"[BaseViewModel] DataItem {info.Name} cannot use id {bindableAttribute.DataItemId} because another item is using it! Skipping until fixed");
                            continue;
                        }

                        AddOrUpdateDataItemFromPropertyInfo(info, bindableAttribute.DataItemId);
                        updatedIds.Add(bindableAttribute.DataItemId);
                        break;
                    }
                }

                attributes.Clear();
            }

            // TODO: do the same thing with methods, but for the BindableAction attribute
            MethodInfo[] methodInfoList = vmType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            attributes.Clear();

            foreach (MethodInfo info in methodInfoList)
            {
                attributes.AddRange(info.GetCustomAttributes());
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is BindableActionAttribute bindableAttribute)
                    {
                        // found a data item! add it to the list so we can serialize it
                        if (updatedIds.Contains(bindableAttribute.DataItemId))
                        {
                            Debug.LogError($"[BaseViewModel] DataItem {info.Name} cannot use id {bindableAttribute.DataItemId} because another item is using it! Skipping until fixed");
                            continue;
                        }

                        AddOrUpdateDataItemFromMethodInfo(info, bindableAttribute.DataItemId);
                        updatedIds.Add(bindableAttribute.DataItemId);
                        break;
                    }
                }

                attributes.Clear();
            }

            ViewModelDataSource dataSource = new ViewModelDataSource();
            // Relying on the ViewModel type name to generate the DataRecord is going to be 
            // problematic if we change the class name. 
            // TODO: figure out handling for this case. Maybe I can serialize the sourceName when
            // the last update happened and auto-rename that record if it exists? That would also
            // help with runtime scenarios where there are two instances and one didn't get
            // updated post-class rename.
            // And alternative would be to serialize a ref to the DataRecord itself, which might be even better
            dataSource.Initialize(this.GetType().ToString(), !IsGlobalSource);
            dataSource.GenerateRecord(RecordPath, dataItemList);

            AssetDatabase.SaveAssetIfDirty(this);

            // TODO: polish: generate a mini report of how many items were added/modified/removed/left untouched
            Debug.Log($"[{this.GetType().ToString()}] Finished updating DataRecord");
#endif
        }


#if UNITY_EDITOR
        private void AddOrUpdateDataItemFromPropertyInfo(PropertyInfo info, int id)
        {
            // We want to avoid re-serializing the same refs over and over again, so first
            // check if we already have an IDataItem object for this ID & type
            bool addNewItem = true;
            IDataItem toRemove = null;
            foreach (IDataItem item in dataItemList)
            {
                if (item.Id == id)
                {
                    // now check the type
                    if (info.PropertyType == item.DataType)
                    {
                        if (item.Name != info.Name)
                        {
                            // call Initialize to refresh the name in case the variable was renamed.
                            item.Initialize(id, info.Name);
                            EditorUtility.SetDirty(this);
                        }
                        addNewItem = false;
                    }
                    else
                    {
                        // the type changed, we need to remove this item from the list and add a new one
                        toRemove = item;
                    }
                    break;
                }
            }

            if (addNewItem)
            {
                if (toRemove != null)
                {
                    dataItemList.Remove(toRemove);
                }

                // now we get to add a new data item to the list
                // first, retrieve the cocnrete implementation of DataItem<T> that
                // is associated with the property type
                if (DataItemTypeCache.TryGetDataItemType(info.PropertyType, out Type dataItemType))
                {
                    // create a new instance of the type
                    IDataItem item = Activator.CreateInstance(dataItemType) as IDataItem;
                    item.Initialize(id, info.Name);

                    dataItemList.Add(item);
                    EditorUtility.SetDirty(this);
                }
            }
        }

        private void AddOrUpdateDataItemFromMethodInfo(MethodInfo info, int id)
        {
            // We want to avoid re-serializing the same refs over and over again, so first
            // check if we already have an IDataItem object for this ID & type
            bool addNewItem = true;
            IDataItem toRemove = null;
            foreach (IDataItem item in dataItemList)
            {
                if (item.Id == id)
                {
                    // now check the type
                    if (typeof(Action) == item.DataType)
                    {
                        if (item.Name != info.Name)
                        {
                            // call Initialize to refresh the name in case the variable was renamed.
                            item.Initialize(id, info.Name);
                            EditorUtility.SetDirty(this);
                        }
                        addNewItem = false;
                    }
                    else
                    {
                        // the type changed, we need to remove this item from the list and add a new one
                        toRemove = item;
                    }
                    break;
                }
            }

            if (addNewItem)
            {
                if (toRemove != null)
                {
                    dataItemList.Remove(toRemove);
                }

                // now we get to add a new data item to the list
                // first, retrieve the cocnrete implementation of DataItem<T> that
                // is associated with the property type
                if (DataItemTypeCache.TryGetDataItemType(typeof(Action), out Type dataItemType))
                {
                    // create a new instance of the type
                    IDataItem item = Activator.CreateInstance(dataItemType) as IDataItem;
                    item.Initialize(id, info.Name);

                    dataItemList.Add(item);
                    EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}