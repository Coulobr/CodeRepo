using UnityEngine;

namespace Grubbit
{
    public class StableChildUIObject : MonoBehaviour
    {
        public Transform stabilizationParent;
        public bool stabilizeRotation;
        public bool stabilizeScale;

        private void Awake()
        {
            if (stabilizationParent == null && transform.parent != null)
            {
                stabilizationParent = transform.parent;
            }
        }

        private void Update()
        {
            if (stabilizationParent != null)
            {
                var trans = transform;

                if (stabilizeScale)
                {
                    trans.localScale = new Vector3(stabilizationParent.localScale.x, 1f, 1f);
                }

                if (stabilizeRotation)
                {
                    trans.eulerAngles = Vector3.zero;
                    trans.localEulerAngles = new Vector3(0f, 0f, -stabilizationParent.localEulerAngles.z * trans.localScale.x);
                }
            }
        }
    }
}