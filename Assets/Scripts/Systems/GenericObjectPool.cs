using System.Collections.Generic;
using UnityEngine;

namespace Systems
{
    public abstract class GenericObjectPool<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private T _prefab;

        public static GenericObjectPool<T> Instance { get; private set; }
        public T Prefab { get => _prefab; set => _prefab = value; }

        [SerializeField] private Queue<T> _objects = new Queue<T>();

        private void Awake()
        {
            Instance = this;
        }

        public T Get(Transform parent = null)
        {
            if (_objects.Count == 0)
            {
                AddObject(parent);
            }
            
            T objectToGet = _objects.Dequeue();
            objectToGet.gameObject.SetActive(true);

            return objectToGet;
        }

        public void ReturnToPool(T objectToReturn)
        {
            objectToReturn.gameObject.SetActive(false);

            if (!_objects.Contains(objectToReturn))
            {
                _objects.Enqueue(objectToReturn);
            }
        }

        private void AddObject(Transform parent = null)
        {
            var newObject = Instantiate(Prefab, parent);
            newObject.gameObject.SetActive(false);
            _objects.Enqueue(newObject);
        }
    }
}