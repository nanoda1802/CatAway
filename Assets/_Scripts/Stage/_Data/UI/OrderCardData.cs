using _Scripts.Stage._Enums;
using AYellowpaper.SerializedCollections;
using PrimeTween;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage._Data.UI
{
    [CreateAssetMenu(fileName = "OrderCardData", menuName = "SO/Stage/UI/OrderCard")]
    public class OrderCardData : ScriptableObject
    {
        [Header("[ Card Theme ]")]
        [SF] private SerializedDictionary<IngredientType, Sprite> ingredientSpriteDic;
        
        [Header("[ Show & Hide Tween ]")]
        [SF] private TweenSettings<float> showAlphaSettings;
        [SF] private TweenSettings<float> hideAlphaSettings;
        [SF] private TweenSettings<float> posSettings;
        [SF] private Ease showEase = Ease.OutBack;
        [SF] private Ease hideEase = Ease.InBack;
        [SF] private Ease moveEase = Ease.OutExpo;
        [SF] private float hideOffsetY = 100f;
        
        [Header("[ Warn Tween ]")]
        [SF] private TweenSettings<float> coverAlphaSettings;
        [SF] private ShakeSettings shakeScaleSettings;
        [SF] private ShakeSettings shakeRotSettings;
        [SF] private float startWarnThreshold = 0.2f;
        
        public TweenSettings<float> ShowAlphaSettings => showAlphaSettings;
        public TweenSettings<float> HideAlphaSettings => hideAlphaSettings;
        public TweenSettings<float> PosSettings => posSettings;
        public Ease ShowEase => showEase;
        public Ease HideEase => hideEase;
        public Ease MoveEase => moveEase;
        public float HideOffsetY => hideOffsetY;
        public TweenSettings<float> CoverAlphaSettings => coverAlphaSettings;
        public ShakeSettings ShakeScaleSettings => shakeScaleSettings;
        public ShakeSettings ShakeRotSettings => shakeRotSettings;
        public float StartWarnThreshold => startWarnThreshold;
        
        public bool TryGetSprite(IngredientType type, out Sprite sprite)
        {
            var hasSprite = ingredientSpriteDic.TryGetValue(type, out sprite);
            return hasSprite;
        }
    }
}