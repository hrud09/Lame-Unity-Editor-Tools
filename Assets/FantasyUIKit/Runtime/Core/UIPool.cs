using System.Collections.Generic;
using UnityEngine;

namespace FantasyUI.Core
{
    /// <summary>
    /// Lightweight generic UI component pool for slots, markers, stat rows, floating healthbars, and toasts.
    /// </summary>
    public class UIPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive = new Stack<T>();
        private readonly List<T> _active = new List<T>();

        public IReadOnlyList<T> ActiveItems => _active;

        public UIPool(T prefab, Transform parent, int prewarm = 0)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarm; i++)
            {
                var inst = Object.Instantiate(_prefab, _parent);
                inst.gameObject.SetActive(false);
                _inactive.Push(inst);
            }
        }

        public T Get()
        {
            T item = _inactive.Count > 0 ? _inactive.Pop() : Object.Instantiate(_prefab, _parent);
            item.transform.SetParent(_parent, false);
            item.transform.SetAsLastSibling();
            item.gameObject.SetActive(true);
            _active.Add(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            _active.Remove(item);
            item.gameObject.SetActive(false);
            _inactive.Push(item);
        }

        public void ReleaseAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var item = _active[i];
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                    _inactive.Push(item);
                }
            }
            _active.Clear();
        }
    }
}
