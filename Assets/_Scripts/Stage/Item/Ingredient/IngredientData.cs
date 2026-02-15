using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Ingredient
{
    [CreateAssetMenu(fileName = "IngredientData", menuName = "SO/Stage/Item/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        [SF] private IngredientType type;
        public IngredientType Type => type;
        
        [Header("[ Model ]")]
        [SF] private Mesh defaultRenderMesh;
        [SF] private Mesh preppedRenderMesh;
        [SF] private Mesh defaultColliderMesh;
        [SF] private Mesh preppedColliderMesh;
        [SF] private Vector3 defaultScale;
        [SF] private Vector3 preppedScale;
        
        public Mesh DefaultRenderMesh => defaultRenderMesh;
        public Mesh PreppedRenderMesh => preppedRenderMesh;
        public Mesh DefaultColliderMesh => defaultColliderMesh;
        public Mesh PreppedColliderMesh => preppedColliderMesh;
        public Vector3 DefaultScale => defaultScale;
        public Vector3 PreppedScale => preppedScale;

        [Header("[ Prep ]")]
        [SF] private float maxProgress;
        [SF] private PrepState maxPrepState;
        
        public float MaxProgress => maxProgress;
        public PrepState MaxPrepState => maxPrepState;
        
        [Header("[ Throw ]")]
        [SF] private float throwForce = 15;
        [SF] private float dampingThreshold = 8;
        [SF] private float dampingRatio = 0.99f;
        [SF] private float detachOffset = 0.25f;
    
        public float ThrowForce => throwForce;
        public float DampingThreshold => dampingThreshold;
        public float DampingRatio => dampingRatio;
        public float DetachOffset => detachOffset;
    }
}