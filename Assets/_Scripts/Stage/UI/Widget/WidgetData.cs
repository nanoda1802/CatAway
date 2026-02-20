using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Widget
{
    public class WidgetData<T> : ScriptableObject where T : WidgetBase
    {
        [SF] private int defaultCount;
        [SF] private int maxCount;
        [SF] private Vector3 offset;
        
        public int DefaultCount => defaultCount;
        public int MaxCount => maxCount;
        public Vector3 Offset => offset;
    }
}