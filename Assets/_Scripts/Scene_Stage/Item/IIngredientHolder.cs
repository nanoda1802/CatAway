namespace _Scripts.Scene_Stage.Item
{
    public interface IIngredientHolder
    {
        public bool HasIngredient { get; }
        
        public bool CanHold(Ingredient.Ingredient ingredient, out string rejectMessage);
        public void Hold(Ingredient.Ingredient ingredient);
        public void ClearHolder();
    }
}