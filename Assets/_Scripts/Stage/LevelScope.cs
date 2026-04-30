using _Scripts.Stage._Data.Item;
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
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LevelInitiator>();
            
            builder.RegisterInstance(ingredientDataList);
            builder.RegisterInstance(plateData);
            
            base.Configure(builder);
        }
    }
}