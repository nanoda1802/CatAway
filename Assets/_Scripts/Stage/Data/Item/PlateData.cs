using System.Collections.Generic;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Data.Item
{
    [CreateAssetMenu(fileName = "PlateData", menuName = "SO/Stage/Item/Plate")]
    public class PlateData : ScriptableObject
    {
        [Header("[ Plating ]")]
        [SF] private int maxPlatingCount = 3;
        [SF] private Vector3 platingLocalPos = new Vector3(0,0.08f,0); 
        [SF] private Vector3 platingLocalScale = new Vector3(0.75f,0.5f,0.75f); 
        [SF] private SerializedDictionary<IngredientType, Mesh> platingMeshDic;
        [SF] private Mesh foodWasteMesh;
        [SF] private Vector3 foodWasteLocalScale = new Vector3(1.5f,0.1f,1f);
        
        public int MaxPlatingCount => maxPlatingCount;
        public Vector3 PlatingLocalPos => platingLocalPos;
        public Vector3 PlatingLocalScale => platingLocalScale;
        public Mesh FoodWasteMesh => foodWasteMesh;
        public Vector3 FoodWasteLocalScale => foodWasteLocalScale;
        
        [Header("[ Prep ]")]
        [SF] private float maxProgress = 2f;
        [SF] private PrepState maxPrepState = PrepState.WellDone;
        
        public float MaxProgress => maxProgress;
        public PrepState MaxPrepState => maxPrepState;
        
        public Mesh GetMesh(IngredientType key)
        {
            return platingMeshDic.GetValueOrDefault(key, foodWasteMesh);
        }
    }
}