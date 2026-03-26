using System;
using _Scripts.Scene_Stage.Enums;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Data
{
    [Serializable]
    public struct MenuInfo
    {
        [SF] private IngredientType recipe;
        [SF] private float duration;
        [SF] private int baseScore;
    
        public IngredientType Recipe => recipe;
        public float Duration => duration;
        public int BaseScore => baseScore;
    }
}