using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Pooling
{
    public class GameObjectPool<T> : IObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool;
        private readonly HashSet<T> _activeObjects;

        public int CountActive => _activeObjects.Count;
        public int CountInactive => _pool.Count;

        public GameObjectPool(T prefab, Transform parent = null, int initialSize = 10)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new Queue<T>();
            _activeObjects = new HashSet<T>();

            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        public T Get()
        {
            T obj;
            
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            obj.gameObject.SetActive(true);
            _activeObjects.Add(obj);
            
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null || !_activeObjects.Contains(obj))
            {
                return;
            }

            obj.gameObject.SetActive(false);
            _activeObjects.Remove(obj);
            _pool.Enqueue(obj);
        }

        public void Clear()
        {
            foreach (var obj in _activeObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }
            _activeObjects.Clear();

            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }
        }

        private T CreateNewObject()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
            return obj;
        }
    }
}
