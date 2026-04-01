using System.Collections.Generic;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Data.Item;
using _Scripts.Scene_Stage.Enums;
using UnityEngine;
using VContainer;

namespace _Scripts.Scene_Stage.Item.Ingredient
{
    public class IngredientProvider : ItemProvider<Ingredient>
    {
        private readonly Dictionary<IngredientType, IngredientData> _dataDic = new ();
        public IngredientType RequiredType { get; private set; }

        [Inject]
        private void Construct(
            StageData stageData,
            IngredientData[] dataList)
        {
            RequiredType = stageData.OrderInfo.RequiredType;
            
            foreach (var data in dataList)
            {
                _dataDic.TryAdd(data.Type, data);
            }
            
            BakeMeshes();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            NetworkManager.PrefabHandler.AddHandler(Info.Prefab, new IngredientPrefabHandler(this));
        }

        private void BakeMeshes()
        {
            MeshColliderCookingOptions options = MeshColliderCookingOptions.CookForFasterSimulation |
                                                        MeshColliderCookingOptions.EnableMeshCleaning |
                                                        MeshColliderCookingOptions.UseFastMidphase |
                                                        MeshColliderCookingOptions.WeldColocatedVertices;
            
            foreach (var data in _dataDic.Values)
            {
                data.BakeColliderMesh(options);
            }
        }
    
        public Ingredient GetIngredient(IngredientType type, Vector3 pos)
        {
            var ingredient = Pool.Get();
            var data = _dataDic.GetValueOrDefault(type, _dataDic[RequiredType]);
            ingredient.InitData(data, data.Type == RequiredType);
            ingredient.transform.parent.transform.position = pos;
            return ingredient;
        }

        // public override void Release(Ingredient item)
        // {
        //     base.Release(item);
        // }
        //
        // public void ReleaseIngredient(Ingredient ingredient)
        // {
        //     if (IsServer) ingredient.NetworkObject.TrySetParent(this.NetworkObject);
        //     Pool.Release(ingredient);
        // }

        public (Mesh, Vector3) GetModelInfo(IngredientType type)
        {
            var data = _dataDic.GetValueOrDefault(type, _dataDic[RequiredType]);
            var modelInfo = data.GetModelInfo();
            return (modelInfo.RenderMesh, modelInfo.Scale);
        }
    }
}