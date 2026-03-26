using System;
using _Scripts.Scene_Stage.Enums;
using UnityEngine;
using RO = _Scripts._Helper.InspectorReadOnlyAttribute;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Data.Level
{
    [Serializable]
    public struct TableSpawnInfo
    {
        [RO, SF] public uint GlobalObjectHashId;
        [RO, SF] public Vector3 Position;
        [RO, SF] public Quaternion Rotation;
        [RO, SF] public IngredientType PantryType;
        public bool IsPantry => PantryType != 0;
    }
}