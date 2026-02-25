using System.Collections.Generic;
using _Scripts.Stage.Item.Ingredient;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Widget.PlatingIcon
{
    [CreateAssetMenu(fileName = "PlatingIconData", menuName = "SO/Stage/UI/Movable/PlatingIcon")]
    public class PlatingIconData : WidgetData<PlatingIconWidget>
    {
        [SF] private SerializedDictionary<IngredientType, Sprite> iconSpriteDic;
        
        public Sprite GetSprite(IngredientType key)
        {
            return iconSpriteDic.GetValueOrDefault(key, null); // [수정] default sprite 정해주기
        }
    }
}