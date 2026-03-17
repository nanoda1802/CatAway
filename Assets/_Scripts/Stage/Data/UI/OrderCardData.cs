using _Scripts.Stage.Item.Ingredient;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Data.UI
{
    [CreateAssetMenu(fileName = "OrderCardData", menuName = "SO/Stage/UI/OrderCard")]
    public class OrderCardData : ScriptableObject
    {
        [SF] private SerializedDictionary<IngredientType, Sprite> ingredientSpriteDic;
        [SF] private TeamTheme coopTheme;
        [SF] private TeamTheme blueTheme;
        [SF] private TeamTheme redTheme;
        
        public TeamTheme CoopTheme => coopTheme;
        public TeamTheme BlueTheme => blueTheme;
        public TeamTheme RedTheme => redTheme;
        
        public bool TryGetSprite(IngredientType type, out Sprite sprite)
        {
            var hasSprite = ingredientSpriteDic.TryGetValue(type, out sprite);
            return hasSprite;
        }
    }
}