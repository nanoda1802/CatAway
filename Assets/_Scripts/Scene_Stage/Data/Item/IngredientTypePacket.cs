using _Scripts.Scene_Stage.Enums;
using Unity.Netcode;

namespace _Scripts.Scene_Stage.Data.Item
{
    public struct IngredientTypePacket : INetworkSerializable
    {
        private IngredientType _ingredientType;
        public IngredientType IngredientType => _ingredientType;

        public IngredientTypePacket(IngredientType ingredientType)
        {
            _ingredientType = ingredientType;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _ingredientType);
        }
    }
}