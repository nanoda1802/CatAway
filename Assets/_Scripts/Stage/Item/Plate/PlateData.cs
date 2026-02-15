using System.Collections.Generic;
using _Scripts.Stage.Item.Ingredient;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Plate
{
    [CreateAssetMenu(fileName = "PlateData", menuName = "SO/Stage/Item/Plate")]
    public class PlateData : ScriptableObject
    {
        [SF] private int maxPlatingCount = 3;
        [SF] private Vector3 platingLocalPos = new Vector3(0,0.08f,0); 
        [SF] private Vector3 platingLocalScale = new Vector3(0.7f,0.5f,0.7f); 
        [SF] private Mesh defaultPlatingMesh;
        [SF] private SerializedDictionary<IngredientType, Mesh> platingMeshDic;
        
        public int MaxPlatingCount => maxPlatingCount;
        public Vector3 PlatingLocalPos => platingLocalPos;
        public Vector3 PlatingLocalScale => platingLocalScale;

        public Mesh GetMesh(IngredientType key)
        {
            return platingMeshDic.GetValueOrDefault(key, defaultPlatingMesh);
        }
    }
}