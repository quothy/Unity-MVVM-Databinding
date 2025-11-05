using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public abstract class BaseLocalViewModel : BaseViewModel
    {
        protected bool isSubscribed = false;
        protected int subscribedSourceId;
        protected int subscribedItemId;
        protected int subscribedIndex = -1;

        protected bool subscribeAfterInitialize = false;

        protected override void Awake()
        {
            base.Awake();

            if (subscribeAfterInitialize)
            {
                Subscribe(subscribedSourceId, subscribedItemId, subscribedIndex);
                subscribeAfterInitialize = false;
            }
        }

        protected override void OnDestroy()
        {
            Unsubscribe();
            base.OnDestroy();
        }

        public void SetBindingData(int sourceId, int itemId)
        {
            if (initialized)
            {
                if (isSubscribed)
                {
                    if (subscribedSourceId == sourceId && subscribedItemId == itemId)
                    {
                        return;
                    }

                    Unsubscribe();
                }
                Subscribe(sourceId, itemId, -1);
            }
            else
            {
                subscribedSourceId = sourceId;
                subscribedItemId = itemId;
                subscribedIndex = -1;
                subscribeAfterInitialize = true;
            }
        }

        public void SetBindingData(int sourceId, int itemId, int index)
        {
            if (initialized)
            {
                if (isSubscribed)
                {
                    if (subscribedSourceId == sourceId && subscribedItemId == itemId)
                    {
                        if (subscribedIndex != index)
                        {
                            subscribedIndex = index;
                            if (DataSourceManager.TryGetDataSource(subscribedSourceId, out IDataSource source))
                            {
                                OnDataUpdated(source, subscribedItemId);
                            }
                        }
                        return;
                    }
                    Unsubscribe();
                }
                Subscribe(sourceId, itemId, index);
            }
            else
            {
                subscribedSourceId = sourceId;
                subscribedItemId = itemId;
                subscribedIndex = index;
                subscribeAfterInitialize = true;
            }
        }

        protected void Subscribe(int sourceId, int itemId, int index)
        {
            if (!isSubscribed)
            {
                subscribedSourceId = sourceId;
                subscribedItemId = itemId;
                subscribedIndex = index;
                DataSourceManager.SubscribeToItem(sourceId, itemId, OnDataUpdated);
                isSubscribed = true;
            }
        }

        public void ClearBindingData()
        {
            Unsubscribe();
            subscribedSourceId = -1;
            subscribedItemId = -1;
            subscribedIndex = -1;
        }

        protected void Unsubscribe()
        {
            if (isSubscribed)
            {
                DataSourceManager.UnsubscribeFromItem(subscribedSourceId, subscribedItemId, OnDataUpdated);
                isSubscribed = false;
            }
        }

        protected virtual void OnDataUpdated(IDataSource source, int itemId) { }
    }
}