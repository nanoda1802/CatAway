using System;
using _Scripts.Stage._Enums;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage._Data
{
    [Serializable]
    public struct OrderInfo
    {
        [SF] private int maxActiveOrderCount;
        [SF] private float newOrderInterval;
        [SF] private IngredientType requiredType;
        [SF] private MenuInfo[] menuList;
   
        public int MaxActiveOrderCount => maxActiveOrderCount;
        public float NewOrderInterval => newOrderInterval;
        public IngredientType RequiredType => requiredType;
        public MenuInfo[] MenuList => menuList;
    }
}