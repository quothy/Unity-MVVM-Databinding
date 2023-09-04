using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class DataBinder : MonoBehaviour
    {
        [SerializeReference]
        private List<IBinder> binders = null;


        private void Awake()
        {
            foreach (IBinder binder in binders)
            {
                binder.Subscribe();
            }
        }

        private void OnDestroy()
        {
            foreach (IBinder binder in binders)
            {
                binder.Unsubscribe();
            }
        }

        public void AddBinder(IBinder binder)
        {
            if (binders == null)
            {
                binders = new List<IBinder>();
            }
            binders.Add(binder);
        }
    }
}