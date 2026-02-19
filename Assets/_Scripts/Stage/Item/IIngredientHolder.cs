namespace _Scripts.Stage.Item
{
    public interface IIngredientHolder
    {
        public bool IsFull { get; }
        public bool HasIngredient { get; }

        public bool TryAdd(Carriable carriable);
        public void ClearHolder();
    }
}