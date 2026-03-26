using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Data.UI
{
    [CreateAssetMenu(fileName = "BoardUiData", menuName = "SO/Stage/UI/Board")]
    public class BoardUiData : ScriptableObject
    {
        [Header("[ ScoreBoard ]")]
        [SF] private Color[] comboColorList;
        [SF] private string comboFormat = "{0} Combo";
        
        public int ComboColorLastIndex => comboColorList.Length - 1;
        public string ComboFormat => comboFormat;
        
        public Color GetComboColor(int idx)
        {
            return (idx < 0 || idx >= comboColorList.Length) 
                ? Color.white
                : comboColorList[idx];
        }

        [Header("[ TimerBoard ]")]
        [SF] private float fillBarDirtyThreshold = 0.01f;
        [SF] private string timerFormat = "{0:D2}:{1:D2}";

        public float FillBarDirtyThreshold => fillBarDirtyThreshold;
        public string TimerFormat => timerFormat;
    }
}