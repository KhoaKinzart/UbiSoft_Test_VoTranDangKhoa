using UnityEngine;

namespace Game.Core.Pooling
{
    public interface IObjectPool<T> where T : Component
    {
        T Get();
        void Return(T obj);
        void Clear();
        int CountActive { get; }
        int CountInactive { get; }
    }
}
