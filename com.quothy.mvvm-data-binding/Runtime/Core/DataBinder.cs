using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class DataBinder : MonoBehaviour
    {
        [SerializeReference]
        private List<IBinder> binders = null;

        /// TODO: Let's try out the concept of "Binding Groups"
        /// Each group is associated with a DataRecord and associated info, but can <summary>
        /// be config'd with a list of IBinders that all draw from that same DataRecord.
        /// 
        /// The goal would be to reduce the amount of duplicated work needed to bind to information
        /// from a single source. It will be important to keep the Inspector UI clean and organized 
        /// when trying out this approach.
        /// </summary>


        private void Awake()
        {
            foreach (IBinder binder in binders)
            {
                binder.Bind();
            }
        }

        private void OnDestroy()
        {
            foreach (IBinder binder in binders)
            {
                binder.Unbind();
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

        public void ReplaceBinderAtIndex(IBinder binder, int index)
        {
            IBinder toReplace = binders[index];
            binders[index] = binder;
        }
    }
}