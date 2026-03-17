using System.Collections.Generic;
using _Scripts.Stage.Item.Ingredient;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using SF = UnityEngine.SerializeField;
using RO = _Scripts.Helper.InspectorReadOnlyAttribute;

namespace _Scripts.Stage.Data
{
    public enum StageMode
    {
        Coop, Comp
    }

    public enum Team
    {
        None, Blue, Red
    }
    
    [CreateAssetMenu(menuName = "SO/Stage/StageInfo", fileName = "StageInfo")]
    public class StageData: ScriptableObject
    {
        [RO, SF] private int id;
        [RO, SF] private StageMode mode;
        [SF] private Sprite thumbnail;
        [SF] private string description;
        [SF] private float duration;
        [RO, SF] private Vector3[] playerSpawnPoints;
        [RO, SF] private List<TableSpawnInfo> tableSpawnInfos;
        [SF] private OrderInfo orderInfo;
    
        public int Id => id;
        public StageMode Mode => mode;
        public Sprite Thumbnail => thumbnail;
        public string Desc => description;
        public float Duration => duration;
        public Vector3[] PlayerSpawnPoints => playerSpawnPoints;
        public List<TableSpawnInfo> TableSpawnInfos => tableSpawnInfos;
        public OrderInfo OrderInfo => orderInfo;

        public StageData Init(
            int idValue,
            StageMode modeValue,
            string descValue,
            float durationValue,
            Vector3[] spawnPoints,
            List<TableSpawnInfo> tableSpawnInfosValue)
        {
            id = idValue;
            mode = modeValue;
            description = descValue;
            duration = durationValue;
            playerSpawnPoints = spawnPoints;
            tableSpawnInfos = tableSpawnInfosValue;
            
            return this;
        }
    }
}