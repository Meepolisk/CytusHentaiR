using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace PoolingObject
{
    public abstract class Manager<T> : MonoBehaviour where T : Object
    {
        protected abstract T prefabs { get; }

        private List<Object> activeList = new List<Object>();
        private Queue<Object> inactiveQueue = new Queue<Object>();

        protected T Spawn(Vector2 _localPos)
        {
            Object obj = null;
            if (inactiveQueue.Count > 0)
            {
                obj = inactiveQueue.Dequeue();
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj = Instantiate(prefabs, transform);
                //obj.Setup(this);
            }
            obj.OnSpawn();
            obj.onDead += ObjectOnDead;
            return obj as T;
        }

        private void ObjectOnDead(Object _object)
        {
            _object.onDead -= ObjectOnDead;
            activeList.Remove(_object);
            inactiveQueue.Enqueue(_object);
            _object.gameObject.SetActive(false);
        }
    }
    public abstract class Object : MonoBehaviour
    {
        public event Action onSpawned;
        public event Action<Object> onDead;

        public virtual void OnSpawn()
        {
            if (onSpawned != null)
                onSpawned();
        }
        public virtual void Kill()
        {
            if (onDead != null)
                onDead(this);
        }
    }
}