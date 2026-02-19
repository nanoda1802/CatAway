using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Player
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "SO/Stage/Player")]
    public class PlayerData : ScriptableObject
    {
        [Header("[ Movement ]")]
        [SF,Range(1f,20f)] private float moveSpeed = 5f;
        [SF,Range(1f,50f)] private float rotSpeed = 25f;
        [SF,Range(1f,50f)] private float dashSpeed = 12f;
        [SF,Range(0f,1f)] private float dashDuration = 0.25f;
        [SF,Range(1f,2f)] private float minSpeedMultiplier = 1f;
        [SF,Range(1f,2f)] private float maxSpeedMultiplier = 1.5f;
        [SF,Range(0f,5f)] private float knockBackImpact = 3f;
        [SF,Range(0f,1f)] private float knockBackDuration = 0.25f;
   
        public float MoveSpeed => moveSpeed;
        public float RotSpeed => rotSpeed;
        public float DashSpeed => dashSpeed;
        public float DashDuration => dashDuration;
        public float MinSpeedMultiplier => minSpeedMultiplier;
        public float MaxSpeedMultiplier => maxSpeedMultiplier;
        public float KnockBackImpact => knockBackImpact;
        public float KnockBackDuration => knockBackDuration;
        
        [Header("[ Interval ]")]
        [SF,Range(0f,1f)] private float dashInterval = 0.5f;
        [SF,Range(0f,1f)] private float carryInterval = 0.2f;
        [SF,Range(0f,1f)] private float interactInterval = 0.2f;
    
        public float DashInterval => dashInterval;
        public float CarryInterval => carryInterval;
        public float InteractInterval => interactInterval;
        
        [Header("[ Detection - Table ]")]
        [SF] private LayerMask tableLayer;
        [SF, Range(0f, 2f)] private float rayDistance = 1.2f;
        
        public LayerMask TableLayer => tableLayer;
        public float RayDistance => rayDistance;
    
        [Header("[ Detection - Item ]")]
        [SF] private LayerMask itemLayer;
        [SF] private int maxDetectionCount = 5;
        [SF] private Vector3 overlapBoxSize = new (0.7f, 0.5f, 0.5f);
        [SF, Range(0f, 5f)] private float overlapBoxOffset = 0.5f;
        
        public LayerMask ItemLayer => itemLayer;
        public int MaxDetectionCount => maxDetectionCount;
        public Vector3 OverlapBoxSize => overlapBoxSize;
        public float OverlapBoxOffset => overlapBoxOffset;
    }
}