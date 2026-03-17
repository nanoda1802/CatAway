using System;
using System.Collections.Generic;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.UI.Widget;
using _Scripts.Stage.UI.Widget.PlatingIcon;
using _Scripts.Stage.UI.Widget.ProgressBar;
using _Scripts.Stage.UI.Widget.TableAlert;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Data
{
    [CreateAssetMenu(fileName = "ProviderData", menuName = "SO/Stage/Provider")]
    public class ProviderData : ScriptableObject
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

        private void OnEnable()
        {
            _providerInfos.Clear();
            
            _providerInfos.Add(typeof(Ingredient), ingredientProviderInfo);
            _providerInfos.Add(typeof(Plate), plateProviderInfo);
            _providerInfos.Add(typeof(Cookware), cookwareProviderInfo);
            
            _providerInfos.Add(typeof(PlatingIconWidget), platingIconProviderInfo);
            _providerInfos.Add(typeof(ProgressBarWidget), progressBarProviderInfo);
            _providerInfos.Add(typeof(TableAlertWidget), tableAlertProviderInfo);
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