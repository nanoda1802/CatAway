using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Table;
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
        [Header("[ Provider ]")]
        [SF] private PlateProvider plateProvider;
        [SF] private IngredientProvider ingredientProvider;

        protected override void Awake()
        {
            if (this.autoInjectGameObjects.Count <= 0) // 만약에 대비 (웬만하면 Table들을 하나의 부모 오브젝트에 묶어두고, 그 부모 오브젝트를 등록)
            {
                var netObjs = FindObjectsByType<NetworkObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var obj in netObjs)
                {
                    if (!obj.TryGetComponent(out IPlacable placable)) continue;
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
            Debug.Log($"LevelScope, Parent : {this.Parent.name}");
            
            builder.RegisterEntryPoint<TableHub>().AsSelf();
                
            builder.RegisterComponent(plateProvider);
            builder.RegisterComponent(ingredientProvider);
            
            var msgOptions = this.Parent.Container.Resolve<MessagePipeOptions>(); // 핵심!!
            builder.RegisterMessageBroker<PlateRackTable>(msgOptions);
            builder.RegisterMessageBroker<PlateReturnTable>(msgOptions);
            
            base.Configure(builder);
        }
    }
}