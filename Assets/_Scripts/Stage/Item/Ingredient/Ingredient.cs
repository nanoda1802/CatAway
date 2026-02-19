using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Ingredient
{
    public class Ingredient : NetworkBehaviour, IPrepable
    {
        private IngredientData _data;
        
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        
        private Carriable _carriable;
        private Throwable _throwable;
        
        private readonly NetworkVariable<IngredientType> _sharedType = new();

        private float _curProgress;
        private PrepState _prepState; // 이걸로 동기화 다시 해야해

        public IngredientType Type => _data.Type;
        public bool IsReady => _prepState == PrepState.WellDone;

        // isServer 매개변수가 있는 메서드들은 호출 시점이 스폰 이전이라서 그렇슴
        
        public void InitStatus(bool isServer, IngredientData data, bool isRequiredIngredient)
        {
            _curProgress = 0;
            _prepState = isRequiredIngredient? PrepState.WellDone : PrepState.Raw;
            
            _data = data;
            SetModel(_data.DefaultRenderMesh,_data.DefaultScale,_data.DefaultColliderMesh, isServer);
        }

        public void InitComponents(bool isServer)
        {
            _meshFilter = this.GetComponentInChildren<MeshFilter>();
            _meshCollider = this.GetComponentInChildren<MeshCollider>();
            
            _meshCollider.isTrigger = !isServer;
            
            _carriable = this.GetComponentInChildren<Carriable>();
            _throwable = this.GetComponentInChildren<Throwable>();
            
            var ingredientRb = this.GetComponentInChildren<Rigidbody>();
            var netTr = this.GetComponentInChildren<NetworkTransform>();
            
            ingredientRb.detectCollisions = isServer;
            
            _carriable?.Construct(ingredientRb);
            _throwable?.Construct(ingredientRb, netTr);
        }

        public override void OnNetworkSpawn()
        {
            if (HasAuthority)
            {
                _throwable?.InitStatus(_data);
                _sharedType.Value = _data.Type;
            }
            
            base.OnNetworkSpawn();
        }

        public float Prepare(int multiplier)
        {
            if (_curProgress >= _data.MaxProgress) return 1;
            
            _curProgress += Time.deltaTime * multiplier;
            return _curProgress / _data.MaxProgress;
        }

        public void OnPrepFinished()
        {
            _prepState = PrepState.WellDone;
            UpdateModelRpc();
        }

        private void SetModel(Mesh renderMesh, Vector3 scale, Mesh colliderMesh, bool isServer)
        {
            _meshFilter.sharedMesh = renderMesh;
            _meshFilter.transform.localScale = scale;
            
            if (!isServer) return;
            
            if (!_meshCollider.convex) _meshCollider.convex = true;
            _meshCollider.sharedMesh = colliderMesh;
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateModelRpc()
        {
            // 몰러,,, 매개변수로 WellDone인지, OverDone인지 받기
            // 그리고 맞는 쪽으로 SetModel
            SetModel(_data.PreppedRenderMesh,_data.PreppedScale,_data.PreppedColliderMesh, IsServer);
        }
    }
}