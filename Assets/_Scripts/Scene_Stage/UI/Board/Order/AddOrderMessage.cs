using _Scripts.Scene_Stage.Enums;
using Unity.Netcode;

namespace _Scripts.Scene_Stage.UI.Board.Order
{
    public struct AddOrderMessage : ITeamMessage, INetworkSerializable
    {
        private Team _team;
        private int _orderId;
        private IngredientType _recipe;
        private float _duration;
        private float _orderTime;
        
        public Team Team => _team;
        public int OrderId => _orderId;
        public IngredientType Recipe => _recipe;
        public float Duration => _duration;
        public float OrderTime => _orderTime;

        public AddOrderMessage(
            Team team,
            int orderId, 
            IngredientType recipe, 
            float duration, 
            float orderTime)
        {
            _team = team;
            _orderId = orderId;
            _recipe = recipe;
            _duration = duration;
            _orderTime = orderTime;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _team);
            serializer.SerializeValue(ref _orderId);
            serializer.SerializeValue(ref _recipe);
            serializer.SerializeValue(ref _duration);
            serializer.SerializeValue(ref _orderTime);
        }
    }
}