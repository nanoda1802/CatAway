using PrimeTween;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage._Data.UI
{
    [CreateAssetMenu(fileName = "BoardUiData", menuName = "SO/Stage/UI/Board")]
    public class BoardUiData : ScriptableObject
    {
        [Header("[ Team Theme ]")]
        [SF] private TeamTheme coopTheme;
        [SF] private TeamTheme blueTheme;
        [SF] private TeamTheme redTheme;
        
        public TeamTheme CoopTheme => coopTheme;
        public TeamTheme BlueTheme => blueTheme;
        public TeamTheme RedTheme => redTheme;
        
        [Header("[ ScoreBoard ]")]
        [SF] private Color[] comboColorList;
        [SF] private string comboFormat = "{0} Combo";
        [SF] private TweenSettings<Color> addScoreColorSettings;
        [SF] private TweenSettings<Color> deductScoreColorSettings;
        [SF] private ShakeSettings scorePunchSettings;
        
        public int ComboColorLastIndex => comboColorList.Length - 1;
        public string ComboFormat => comboFormat;
        public TweenSettings<Color> AddScoreColorSettings => addScoreColorSettings;
        public TweenSettings<Color> DeductScoreColorSettings => deductScoreColorSettings;
        public ShakeSettings ScorePunchSettings => scorePunchSettings;
        
        public Color GetComboColor(int idx)
        {
            return (idx < 0 || idx >= comboColorList.Length) 
                ? Color.white
                : comboColorList[idx];
        }

        [Header("[ TimerBoard ]")] 
        [SF] private int feverTime = 60;
        [SF] private float fillBarDirtyThreshold = 0.01f;
        [SF] private string timerFormat = "{0:D2}:{1:D2}";
        [SF] private TweenSettings<Color> timerColorSettings;
        [SF] private ShakeSettings timerPunchSettings;
        
        public int FeverTime => feverTime;
        public float FillBarDirtyThreshold => fillBarDirtyThreshold;
        public string TimerFormat => timerFormat;
        public TweenSettings<Color> TimerColorSettings => timerColorSettings;
        public ShakeSettings TimerPunchSettings => timerPunchSettings;
    }
}