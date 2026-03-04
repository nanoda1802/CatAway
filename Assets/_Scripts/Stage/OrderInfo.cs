using System;
using _Scripts.Stage.Item.Ingredient;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage
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