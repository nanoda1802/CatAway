using System.Collections.Generic;
using _Scripts.Scene_Stage.Enums;
using AYellowpaper.SerializedCollections;
using PrimeTween;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Data.UI
{
    [CreateAssetMenu(fileName = "WidgetData", menuName = "SO/Stage/UI/Widget")]
    public class WidgetData : ScriptableObject
    {
        [Header("[ ProgressBar ]")]
        [SF] private Vector3 progressBarOffset = new Vector3(0,1.2f,0);
        
        public Vector3 ProgressBarOffset => progressBarOffset;
        
        [Header("[ TableAlert ]")]
        [SF] private Vector3 tableAlertOffset = new Vector3(0, 1f, 0);
        [SF] private ShakeSettings alertTweenSettings;
        public Vector3 TableAlertOffset => tableAlertOffset;
        public ShakeSettings AlertTweenSettings => alertTweenSettings;

        [Header("[ PlatingIcon ]")]
        [SF] private Vector3 platingIconOffset = new Vector3(0,0.5f,0);
        [SF] private SerializedDictionary<IngredientType, Sprite> iconSpriteDic;
        [SF] private Sprite defaultIconSprite;

        public Vector3 PlatingIconOffset => platingIconOffset;
        public Sprite GetIconSprite(IngredientType type) => iconSpriteDic.GetValueOrDefault(type, defaultIconSprite);
        
        [Header("[ Toast ]")]
        [SF] private Vector3 toastOffset = new Vector3(0,1f,0);
        [SF] private Color positiveColor;
        [SF] private Color negativeColor;
        [SF] private TweenSettings<float> toastAlphaSettings; 
        [SF] private float toastMoveDist; 
        
        public Vector3 ToastOffset => toastOffset;
        public Color PositiveColor => positiveColor;
        public Color NegativeColor => negativeColor;
        public TweenSettings<float> ToastAlphaSettings => toastAlphaSettings;
        public float ToastMoveDist => toastMoveDist;
    }
}