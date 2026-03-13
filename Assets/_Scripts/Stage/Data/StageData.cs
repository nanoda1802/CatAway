using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage
{
    public enum StageMode
    {
        Coop, Comp
    }

    public enum Team
    {
        None, Blue, Red
    }
    
    [CreateAssetMenu(menuName = "SO/Stage/StageInfo", fileName = "StageInfo")]
    public class StageData: ScriptableObject
    {
        [SF] private int id;
        [SF] private StageMode mode;
        [SF] private Sprite thumbnail;
        [SF] private string description;
        [SF] private float duration;
        [SF] private OrderInfo orderInfo;
    
        public int Id => id;
        public StageMode Mode => mode;
        public Sprite Thumbnail => thumbnail;
        public string Desc => description;
        public float Duration => duration;
        public OrderInfo OrderInfo => orderInfo;
    }
}