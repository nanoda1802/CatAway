using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Ingredient
{
    public class Ingredient : NetworkBehaviour, IPrepable
    {
        [SF] private IngredientData data;
        
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        
        private Carriable _carriable;
        private Throwable _throwable;
        
        private readonly NetworkVariable<IngredientType> _sharedType = new();

        private float _curProgress;
        private PrepState _prepState;

        public IngredientType Type => data.Type;

        private void Awake() // [임시]
        {
            InitComponents();
        }

        public void InitComponents()
        {
            _meshFilter = this.GetComponentInChildren<MeshFilter>();
            _meshCollider = this.GetComponentInChildren<MeshCollider>();
            
            _carriable = this.GetComponentInChildren<Carriable>();
            _throwable = this.GetComponentInChildren<Throwable>();
         
            var rb = this.GetComponentInChildren<Rigidbody>();
            var netTr = this.GetComponentInChildren<NetworkTransform>();
            
            _carriable?.Construct(rb);
            _throwable?.Construct(this.data, rb, netTr);
        }

        public override void OnNetworkSpawn()
        {
            if (HasAuthority) _sharedType.Value = data.Type;
            
            base.OnNetworkSpawn();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!HasAuthority) return;
            if (!_throwable.IsThrowing) return;
            
            _throwable.StopThrowing();
        }

        public float Prepare()
        {
            return _curProgress / data.MaxProgress;
        }

        private void OnPrepDone()
        {
            SetModel(data.PreppedRenderMesh,data.PreppedScale,data.PreppedColliderMesh);
        }

        public void Reset()
        {
            _curProgress = 0;

            ResetModelClientRpc();
            
            _carriable.Detach();
            _throwable.StopThrowing();
        }

        private void SetModel(Mesh renderMesh, Vector3 scale, Mesh colliderMesh)
        {
            _meshFilter.sharedMesh = renderMesh;
            _meshFilter.transform.localScale = scale;
            _meshCollider.sharedMesh = colliderMesh;
        }

        [ClientRpc]
        private void ResetModelClientRpc()
        {
            SetModel(data.DefaultRenderMesh,data.DefaultScale,data.DefaultColliderMesh);
        }
    }
}