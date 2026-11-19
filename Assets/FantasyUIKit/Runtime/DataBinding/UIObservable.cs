using System;
using System.Collections.Generic;

namespace FantasyUI.Data
{
    /// <summary>
    /// Lightweight reactive observable value for zero-coupling gameplay-to-UI bindings.
    /// </summary>
    public class UIObservable<T>
    {
        private T _value;
        public event Action<T> OnChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;
                _value = value;
                OnChanged?.Invoke(_value);
            }
        }

        public UIObservable(T initial = default) => _value = initial;

        public void SetSilently(T value) => _value = value;
        public void Notify() => OnChanged?.Invoke(_value);
    }
}
