using _Scripts.Scene_Stage.Enums;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Result.Data
{
    [CreateAssetMenu(menuName = "SO/UI/ResultView",  fileName = "ResultView")]
    public class ResultViewData : ScriptableObject
    {
        [Header("[ Timer ]")]
        [SF] private float duration = 10f;
        [SF] private float spareTimeAfterSkip = 5f;
        [SF] private string timerFormat = "{0:D2}:{1:D2}";
        [SF] private Color spareTimeColor;
        
        public float Duration => duration;
        public float SpareTimeAfterSkip => spareTimeAfterSkip;
        public string TimerFormat => timerFormat;
        public Color SpareTimeColor => spareTimeColor;

        [Header("[ Skip Vote ]")]
        [SF] private Sprite checkIcon;
        [SF] private Sprite crossIcon;
        [SF] private Color checkColor;
        [SF] private Color crossColor;
        
        public Sprite CheckIcon => checkIcon;
        public Sprite CrossIcon => crossIcon;
        public Color CheckColor => checkColor;
        public Color CrossColor => crossColor;
        
        [Header("[ Member Card ]")] 
        [SF] private float offsetY = 1f;
        [SF] private int defaultCount = 5;
        [SF] private int maxCount = 10;
        [SF] private SerializedDictionary<Team, Color> namePlateColors;
        
        public float OffsetY => offsetY;
        public int DefaultCount => defaultCount;
        public int MaxCount => maxCount;

        public Color GetNamePlateColor(Team team)
        {
            return namePlateColors[team];
        }
    }
}