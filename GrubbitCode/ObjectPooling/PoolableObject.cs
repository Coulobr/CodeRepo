using UnityEngine;

#region ReSharper Comments
// ReSharper disable IdentifierTypo
// ReSharper disable ParameterHidesMember
#endregion

namespace Grubbit
{
    public class PoolableObject : MonoBehaviour
    {
        public int id;
        protected ObjectPooler parentObjectPooler;

        protected virtual void OnDestroy()
        {
            ClearElement();
        }

        /// <summary>
        /// Clears the poolable object.
        /// </summary>
        public virtual void ClearElement() { }

        /// <summary>
        /// Sets up the poolable object for use.
        /// </summary>
        public virtual void SetupElement(ObjectPooler desiredParentObjectPooler)
        {
            if (desiredParentObjectPooler != null)
            {
                parentObjectPooler = desiredParentObjectPooler;
            }
        }

        /// <summary>
        /// Updates the current state of the poolable object.
        /// </summary>
        public virtual void UpdateElement() { }

        /// <summary>
        /// Determines if the asset has all of the required components assigned.
        /// </summary>
        protected virtual bool HasRequiredAssets()
        {
            return true;
        }
    }
}