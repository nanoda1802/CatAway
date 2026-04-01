using System;
using System.Collections.Generic;
using _Scripts.Scene_Stage.Item;
using _Scripts.Scene_Stage.Item.Cookware;
using _Scripts.Scene_Stage.Item.Ingredient;
using _Scripts.Scene_Stage.Item.Plate;
using _Scripts.Scene_Stage.UI.Widget;
using _Scripts.Scene_Stage.UI.Widget.PlatingIcon;
using _Scripts.Scene_Stage.UI.Widget.ProgressBar;
using _Scripts.Scene_Stage.UI.Widget.TableAlert;
using _Scripts.Scene_Stage.UI.Widget.Toast;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Data.Level
{
    [CreateAssetMenu(fileName = "ProviderData", menuName = "SO/Stage/Provider")]
    public class ProviderData : ScriptableObject, IInitializable
    {
        private readonly Dictionary<Type, IProviderInfo> _providerInfos = new();
        
        [Header("[ Item ]")]
        [SF] private ProviderInfo<NetworkObject> ingredientProviderInfo;
        [SF] private ProviderInfo<NetworkObject> plateProviderInfo;
        [SF] private ProviderInfo<NetworkObject> cookwareProviderInfo;

        [Header("[ Widget ]")]
        [SF] private ProviderInfo<PlatingIconWidget> platingIconProviderInfo;
        [SF] private ProviderInfo<ProgressBarWidget> progressBarProviderInfo;
        [SF] private ProviderInfo<TableAlertWidget> tableAlertProviderInfo;
        [SF] private ProviderInfo<ToastWidget> toastProviderInfo;

        public void Initialize()
        {
            _providerInfos.Clear();
            
            _providerInfos.Add(typeof(Ingredient), ingredientProviderInfo);
            _providerInfos.Add(typeof(Plate), plateProviderInfo);
            _providerInfos.Add(typeof(Cookware), cookwareProviderInfo);
            
            _providerInfos.Add(typeof(PlatingIconWidget), platingIconProviderInfo);
            _providerInfos.Add(typeof(ProgressBarWidget), progressBarProviderInfo);
            _providerInfos.Add(typeof(TableAlertWidget), tableAlertProviderInfo);
            _providerInfos.Add(typeof(ToastWidget), toastProviderInfo);
        }

        public ProviderInfo<NetworkObject> GetItemProviderInfo<T>() where T : Carriable
        {
            _providerInfos.TryGetValue(typeof(T), out var providerInfo);
            return providerInfo as ProviderInfo<NetworkObject>;
        }
        
        public ProviderInfo<T> GetWidgetProviderInfo<T>() where T : WidgetBase
        {
            _providerInfos.TryGetValue(typeof(T), out var providerInfo);
            return providerInfo as ProviderInfo<T>;
        }
    }
}