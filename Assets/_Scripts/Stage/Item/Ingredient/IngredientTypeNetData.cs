using Unity.Netcode;

namespace _Scripts.Stage.Item.Ingredient
{
    public struct IngredientTypeNetData : INetworkSerializable
    {
        private IngredientType _ingredientType;
        public IngredientType IngredientType => _ingredientType;

        public IngredientTypeNetData(IngredientType ingredientType)
        {
            _ingredientType = ingredientType;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _ingredientType);
        }
    }
}