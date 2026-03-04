using _Scripts.Stage.Item.Ingredient;
using Unity.Netcode;

namespace _Scripts.Stage.UI.Board.Order
{
    public struct AddOrderMessage : ITeamMessage, INetworkSerializable
    {
        private Team _team;
        private int _id;
        private IngredientType _recipe;
        private float _duration;
        private float _orderTime;
        
        public Team Team => _team;
        public int Id => _id;
        public IngredientType Recipe => _recipe;
        public float Duration => _duration;
        public float OrderTime => _orderTime;

        public AddOrderMessage(
            Team team,
            int id, 
            IngredientType recipe, 
            float duration, 
            float orderTime)
        {
            _team = team;
            _id = id;
            _recipe = recipe;
            _duration = duration;
            _orderTime = orderTime;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _team);
            serializer.SerializeValue(ref _id);
            serializer.SerializeValue(ref _recipe);
            serializer.SerializeValue(ref _duration);
            serializer.SerializeValue(ref _orderTime);
        }
    }
}