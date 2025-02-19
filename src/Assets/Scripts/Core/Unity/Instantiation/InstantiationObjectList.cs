﻿using Core.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Unity
{
    public class InstantiationObjectList<T> : IInstantiationObjectList<T>
        where T : Component
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private T _prefab;
        private readonly List<T> _instantiated = new List<T>();

        public T Instantiate()
        {
            if (!_parent.gameObject.activeSelf)
                _parent.gameObject.SetActive(true);

            var instantiated = GameObject.Instantiate(_prefab, _parent);
            _instantiated.Add(instantiated);

            return instantiated;
        }

        public T Instantiate(Vector3 position)
        {
            if (!_parent.gameObject.activeSelf)
                _parent.gameObject.SetActive(true);

            var instantiated = GameObject.Instantiate(_prefab, position, Quaternion.identity, _parent);
            _instantiated.Add(instantiated);

            return instantiated;
        }

        public void Destroy()
        {
            _instantiated.Destroy();
            _instantiated.Clear();
        }

        public void Destroy(int index, int count = 1)
        {
            if (count < 1 || _instantiated.Count == 0)
                return;

            if (count > _instantiated.Count - index)
                count = _instantiated.Count - index;
            
            Enumerable.Range(index, count).ForEachReversed(i =>
            {
                var inst = _instantiated[index];
                if (inst == null)
                    return;

                inst.Destroy();
                _instantiated.RemoveAt(index);
            });
        }

        public void Destroy(T @object)
        {
            var inst = _instantiated.FirstOrDefault(inst => object.ReferenceEquals(@object, inst));
            if (inst == null)
                return;

            inst.Destroy();
            _instantiated.Remove(inst);
        }

        public T Object => _instantiated.FirstOrDefault();
        public IEnumerable<T> Objects => _instantiated;
        public Transform Parent => _parent;
        public T Prefab => _prefab;
    }
}
