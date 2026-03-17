using _Scripts.Stage.Data;
using _Scripts.Stage.Item.Ingredient;
using UnityEngine;

namespace _Scripts.Stage.UI.Board.Order
{
    public class OrderStatus
    {
        private MenuInfo _menuInfo;
        private float _expireTimer;

        public int Id { get; private set; }
        public IngredientType Recipe => _menuInfo.Recipe;
        public float Duration => _menuInfo.Duration;
        public int BaseScore => _menuInfo.BaseScore;
        public float RemainingRatio => _expireTimer / Duration;

        public OrderStatus InitStatus(int orderId, MenuInfo menuInfo)
        {
            Id = orderId;
            _menuInfo = menuInfo;
            _expireTimer = menuInfo.Duration;
            return this;
        }
        
        public bool UpdateTimer()
        {
            _expireTimer -= Time.deltaTime;
            return _expireTimer <= 0;
        }
    }
}