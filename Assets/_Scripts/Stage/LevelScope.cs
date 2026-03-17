using _Scripts.Lobby;
using _Scripts.Stage.Data.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player;
using _Scripts.Stage.Table;
using _Scripts.Stage.Table.Placable;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage
{
    public class LevelScope : LifetimeScope
    {
        [Header("[ Data ]")]
        [SF] private IngredientData[] ingredientDataList;
        [SF] private PlateData plateData;
        
        protected override void Awake()
        {
            if (this.autoInjectGameObjects.Count <= 0) // 만약에 대비 (웬만하면 Table들을 하나의 부모 오브젝트에 묶어두고, 그 부모 오브젝트를 등록)
            {
                var netObjs = FindObjectsByType<NetworkObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var obj in netObjs)
                {
                    if (!obj.CompareTag("Table")) continue;
                    autoInjectGameObjects.Add(obj.gameObject);
                }
            }

            if (!this.autoRun)
            {
                this.autoRun = true;
            }
            
            base.Awake();
        }
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LevelInitiator>();
            
            builder.RegisterInstance(ingredientDataList);
            builder.RegisterInstance(plateData);
            
            base.Configure(builder);
        }
    }
}