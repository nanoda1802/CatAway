using _Scripts.Scene_Stage.Enums;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Data.Item
{
    [CreateAssetMenu(fileName = "IngredientData", menuName = "SO/Stage/Item/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        [SF] private IngredientType type;
        public IngredientType Type => type;
        
        [Header("[ Model ]")]
        [SF] private ModelInfo defaultModelInfo;
        [SF] private ModelInfo preppedModelInfo;
        [SF] private ModelInfo burnedModelInfo;

        [Header("[ Prep ]")]
        [SF] private float maxProgress;
        [SF] private PrepState maxPrepState;
        
        public float MaxProgress => maxProgress;
        public PrepState MaxPrepState => maxPrepState;
        
        [Header("[ Throw ]")]
        [SF] private float throwForce = 17;
        [SF] private float dampingThreshold = 10;
        [SF] private float dampingRatio = 0.97f;
        [SF] private float validVelocityCutOff = 25f;
    
        public float ThrowForce => throwForce;
        public float DampingThreshold => dampingThreshold;
        public float DampingRatio => dampingRatio;
        public float ValidVelocityCutOff => validVelocityCutOff;

        public void BakeColliderMesh(MeshColliderCookingOptions options)
        {
            if (defaultModelInfo.ColliderMesh != null)
            {
                Physics.BakeMesh(defaultModelInfo.ColliderMesh.GetInstanceID(), true, options);
            }

            if (preppedModelInfo.ColliderMesh != null)
            {
                Physics.BakeMesh(preppedModelInfo.ColliderMesh.GetInstanceID(), true, options);
            }

            if (burnedModelInfo.ColliderMesh != null)
            {
                Physics.BakeMesh(burnedModelInfo.ColliderMesh.GetInstanceID(), true, options);
            }
        }

        public ModelInfo GetModelInfo(PrepState prepState = PrepState.Raw)
        {
            var modelInfo = prepState switch
            {
                PrepState.Raw => defaultModelInfo,
                PrepState.WellDone => preppedModelInfo,
                PrepState.OverDone => burnedModelInfo,
                _ => default
            };
            
            return modelInfo;
        }
    }
}