using System;
using _Scripts.Stage.Item.Ingredient;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Data
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