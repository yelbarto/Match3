using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace PuzzleGame.Util.Pool
{
    public class GameObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private readonly CancellationTokenSource _disposeCts;
        private int Capacity { get; set; }
        private int MaxCapacity { get; set; }
        private readonly Queue<T> _pool = new();

        public GameObjectPool(T prefabName, int initialCapacity = 0,
            int maxCapacity = int.MaxValue)
        {
            Capacity = initialCapacity;
            MaxCapacity = maxCapacity;
            
            if(initialCapacity > 0)
                WarmUp(initialCapacity);
            Prefab = prefabName;
            _disposeCts = new CancellationTokenSource();
        }
        
        public void WarmUp(int count)
        {
            for (var i = 0; i < count; i++)
                _pool.Enqueue(Create());
            Capacity += count;
        }

        public GameObjectPool(T prefab, Transform poolParent,
            int initialCapacity = 0, int maxCapacity = int.MaxValue) : 
            this(prefab, initialCapacity, maxCapacity)
        {
            PoolParent = poolParent;
        }

        public GameObjectPool(T prefabName,
            string parentName, int initialCapacity = 0, int maxCapacity = int.MaxValue) : 
            this(prefabName, new GameObject(parentName).transform, initialCapacity, 
                maxCapacity)
        {
        }

        public Transform PoolParent { get; }

        private T Prefab { get; }
        private int AvailableCount => _pool.Count;

        public T Get(Transform parent)
        {
            return Get(parent, Vector3.zero, Quaternion.identity);
        }
        public void Resize(int newCapacity)
        {
            if(newCapacity > MaxCapacity)
                throw new Exception("New capacity is greater than max capacity");
            if(newCapacity < 0)
                throw new Exception("New capacity is less than 0");
            if(newCapacity == Capacity)
                return;
            if(newCapacity > Capacity)
            {
                var count = newCapacity - Capacity;
                WarmUp(count);
            }
            else
            {
                var count = Capacity - newCapacity;
                for (var i = 0; i < count; i++)
                    _pool.Dequeue();
            }
            
            Capacity = newCapacity;
        }
        
        public T Get()
        {
            if(AvailableCount == 0)
            {
                Resize(Capacity + 1);
            }
            var item = _pool.Dequeue();
            item.OnSpawn();
            return item;
        }

        public T Get(Transform parent, Vector3 position, Quaternion rotation)
        {
            var instance = Get();
            var transform = instance.transform;
            transform.SetParent(parent);
            transform.localPosition = position;
            transform.localRotation = rotation;
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Return(T instance)
        {
            instance.gameObject.SetActive(false);
            var instanceTransform = instance.transform;
            if (instanceTransform.parent != PoolParent)
                instanceTransform.SetParent(PoolParent);
            _pool.Enqueue(instance);
            if(instance is IPoolable poolableItem)
                poolableItem.OnDespawn();
        }
        
        public void LoadPrefab(int initialCount = 20)
        {
            WarmUp(initialCount);
        }
        
        protected T Create()
        {
            var instance = GameObject.Instantiate(Prefab, PoolParent);
            instance.gameObject.SetActive(false);
            return instance.GetComponent<T>();
        }

        public void Dispose()
        {
            foreach (var item in _pool)
                if (item is IDisposable disposable)
                    disposable.Dispose();
            
            _pool.Clear();
            _disposeCts?.Cancel();
        }
    }
}