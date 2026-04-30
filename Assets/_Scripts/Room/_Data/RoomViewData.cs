using _Scripts.Stage._Enums;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Room._Data
{
    [CreateAssetMenu(menuName = "SO/UI/RoomView",  fileName = "RoomView")]
    public class RoomViewData : ScriptableObject
    {
        [SF] private SerializedDictionary<StageMode,Color> stageModeColorDic;

        [Header("[ Icon ]")]
        [SF] private Sprite hostIcon;
        [SF] private Sprite checkIcon;
        [SF] private Sprite crossIcon;
        [SF] private Color hostColor;
        [SF] private Color checkColor;
        [SF] private Color crossColor;

        [Header("[ Member Card ]")] 
        [SF] private float offsetY = 1.5f;
        [SF] private int defaultCount = 5;
        [SF] private int maxCount = 10;
        
        public Sprite HostIcon => hostIcon;
        public Sprite CheckIcon => checkIcon;
        public Sprite CrossIcon => crossIcon;
        public Color HostColor => hostColor;
        public Color CheckColor => checkColor;
        public Color CrossColor => crossColor;
        
        public float OffsetY => offsetY;
        public int DefaultCount => defaultCount;
        public int MaxCount => maxCount;

        public Color GetThemeColor(StageMode stageMode)
        {
            return stageModeColorDic[stageMode];
        }
    }
}