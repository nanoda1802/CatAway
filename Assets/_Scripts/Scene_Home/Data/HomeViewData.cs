using PrimeTween;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Home.Data
{
    [CreateAssetMenu(menuName = "SO/UI/HomeView",  fileName = "HomeView")]
    public class HomeViewData : ScriptableObject
    {
        [SF] private int shakeInterval = 3000;
        [SF] private ShakeSettings scaleSetting;
        [SF] private ShakeSettings rotSetting;
        
        public int ShakeInterval => shakeInterval;
        public ShakeSettings ScaleSetting => scaleSetting;
        public ShakeSettings RotSetting => rotSetting;
    }
}