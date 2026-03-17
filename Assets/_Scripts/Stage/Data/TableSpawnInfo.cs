using System;
using _Scripts.Helper;
using _Scripts.Stage.Item.Ingredient;
using UnityEngine;
using RO = _Scripts.Helper.InspectorReadOnlyAttribute;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Data
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