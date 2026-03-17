using System;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Data
{
    [Serializable]
    public class ProviderInfo<T> : IProviderInfo
    {
        [SF] private T prefab;
        [SF] private string objNamePrefix;
        [SF] private int defaultCount;
        [SF] private int maxCount;
        
        public T Prefab => prefab;
        public string ObjNamePrefix => objNamePrefix;
        public int DefaultCount => defaultCount;
        public int MaxCount => maxCount;
    }
}