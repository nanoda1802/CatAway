namespace _Scripts.Stage.Item
{
    public interface IIngredientHolder
    {
        public bool HasIngredient { get; }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream

        public bool TryAdd(Carriable carriable);
=======
        public bool CanHold(Ingredient.Ingredient ingredient, out string rejectMessage);
        public void Hold(Ingredient.Ingredient ingredient);
>>>>>>> Stashed changes
=======
        public bool CanHold(Ingredient.Ingredient ingredient, out string rejectMessage);
        public void Hold(Ingredient.Ingredient ingredient);
>>>>>>> Stashed changes
=======
        public bool CanHold(Ingredient.Ingredient ingredient, out string rejectMessage);
        public void Hold(Ingredient.Ingredient ingredient);
>>>>>>> Stashed changes
=======
        public bool CanHold(Ingredient.Ingredient ingredient, out string rejectMessage);
        public void Hold(Ingredient.Ingredient ingredient);
>>>>>>> Stashed changes
        public void ClearHolder();
    }
}