using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Grubbit
{
    public class ObjectPooler : MonoBehaviour
    {
        public PoolableObject poolableObjectTemplate;
        public Transform usedContainer;
        public Transform unusedContainer;
        public string desiredObjectName = "PoolableObject";
        public int numOfObjectsToMake = 10;
        public bool canExpandCount;
        public bool setAsFirstSibling;

        public List<PoolableObject> allPoolableObjects = new List<PoolableObject>();
        public List<PoolableObject> usedPoolableObjects = new List<PoolableObject>();
		protected bool generatedAssets;

        protected virtual void Start()
        {
            if (!generatedAssets)
            {
                GenerateAssets();
            }
        }

        public virtual void SetupElement()
        {
            if (!generatedAssets)
            {
                GenerateAssets();
            }
        }

        public virtual void UpdateElement() { }

        public virtual void ResetElement()
        {
            for (var i = usedPoolableObjects.Count - 1; i >= 0; --i)
            {
                DeactivatePoolableObject(usedPoolableObjects[i]);
            }
        }

        protected virtual bool HasRequiredAssets()
        {
            return true;
        }

        public virtual PoolableObject GetFreeObject()
        {
            var freePoolableObjects = allPoolableObjects.Except(usedPoolableObjects).ToList();

            if (freePoolableObjects.Count == 0)
            {
                if (canExpandCount)
                {
                    freePoolableObjects.Add(GenerateSingleAsset(allPoolableObjects.Count));
                }
                else
                {
                    return null;
                }
            }

            return freePoolableObjects[freePoolableObjects.Count - 1];
        }

        public virtual PoolableObject ActivatePoolableObject(PoolableObject desiredPoolableObject)
        {
            desiredPoolableObject.gameObject.SetActive(true);
            desiredPoolableObject.transform.SetParent(usedContainer);

            if (setAsFirstSibling)
            {
                desiredPoolableObject.transform.SetAsFirstSibling();
            }
            else
            {
                desiredPoolableObject.transform.SetAsLastSibling();
            }

            usedPoolableObjects.Add(desiredPoolableObject);
            return desiredPoolableObject;
        }

        public virtual void DeactivatePoolableObject(PoolableObject desiredPoolableObject)
        {
            desiredPoolableObject.gameObject.SetActive(false);
            desiredPoolableObject.transform.SetParent(unusedContainer);
            desiredPoolableObject.ClearElement();

            if (usedPoolableObjects.Contains(desiredPoolableObject))
            {
                usedPoolableObjects.Remove(desiredPoolableObject);
            }
        }

        public virtual PoolableObject GetPoolableObjectByID(int id)
        {
            return allPoolableObjects.FirstOrDefault(t => t.id == id);
        }

        protected virtual PoolableObject GenerateSingleAsset(int num)
        {
            var newPoolableObject = Instantiate(poolableObjectTemplate, unusedContainer);
            newPoolableObject.name = $"{desiredObjectName}_{num}";
            newPoolableObject.gameObject.SetActive(false);
            newPoolableObject.id = num;
            newPoolableObject.SetupElement(this);
            newPoolableObject.ClearElement();
            allPoolableObjects.Add(newPoolableObject);
            return newPoolableObject;
        }

        protected virtual void GenerateAssets()
        {
            for (var i = 0; i < numOfObjectsToMake - 1; ++i)
            {
                GenerateSingleAsset(i);
            }

            poolableObjectTemplate.name = $"{desiredObjectName}_{numOfObjectsToMake}";
            poolableObjectTemplate.id = numOfObjectsToMake;
            poolableObjectTemplate.gameObject.SetActive(false);
            poolableObjectTemplate.transform.SetParent(unusedContainer);
            poolableObjectTemplate.SetupElement(this);
            poolableObjectTemplate.ClearElement();
            allPoolableObjects.Add(poolableObjectTemplate);
            generatedAssets = true;
        }
    }
}