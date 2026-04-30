using System.Threading;
using _Scripts.Stage._Data.Item;
using _Scripts.Stage._Enums;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

namespace _Scripts.Stage.Item.Ingredient
{
    public class Ingredient : Carriable, IPrepable
    {
        // Data
        private IngredientData _data;
        // Component
        private MeshFilter _meshFilter;
        private NetworkTransform _networkTr;
        // Status
        private float _curProgress;
        private PrepState _curState;
        // Caching
        private CancellationTokenSource _throwCts;
        // Property
        public IngredientType Type => _data.Type;
        public bool IsRaw => _curState == PrepState.Raw;
        public bool IsWellPrepped => _curState == PrepState.WellDone;
        public bool IsMaxPrepped => _curState == _data.MaxPrepState;
        public bool IsThrowing { get; private set; }
        
        [Inject]
        public void Construct()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _networkTr = this.transform.parent.GetComponent<NetworkTransform>();
        }
        
        public override void Despawn()
        {
            if (!IsServer) return;

            var provider = StageHub.FetchProvider<IngredientProvider>();
            provider.Release(this);
            
            base.Despawn();
        }

        public void InitData(IngredientData data, bool isRequiredIngredient)
        {
            _data = data;
            
            _curProgress = 0;
            _curState = isRequiredIngredient? PrepState.WellDone : PrepState.Raw;
        }

        private void SetModel(Mesh renderMesh, Vector3 scale, Mesh colliderMesh)
        {
            _meshFilter.sharedMesh = renderMesh;
            _meshFilter.transform.localScale = scale;
            
            if (HasAuthority) ItemCollider.sharedMesh = colliderMesh;
        }

        #region NGO 관련 메서드
        public override void OnNetworkSpawn()
        {
            if (HasAuthority)
            {
                UpdateModelRpc(_curState);
            }
            
            base.OnNetworkSpawn();
        }

        protected override void OnAttaching()
        {
            base.OnAttaching();
            CancelThrowing();
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateModelRpc(PrepState prepState = PrepState.Raw)
        {
            var info = _data.GetModelInfo(prepState);
            if (!info.IsValid)
            {
                Debug.LogError($"갱신할 ModelInfo가 유효하지 않습니다. [{_data.name}] 를 점검하세요.");
                return;
            }
            
            SetModel(info.RenderMesh, info.Scale, info.ColliderMesh);
        }
        #endregion

        #region Throw 관련 메서드
        private void CancelThrowing()
        {
            if (!IsThrowing) return;
            
            _throwCts?.Cancel();
            _throwCts?.Dispose();
            _throwCts = null;
            
            IsThrowing = false;
        }

        public async UniTaskVoid Throw(Vector3 originPoint, Quaternion originRot, Vector3 throwDir)
        {
            if (IsThrowing) return;
            
            IsThrowing = true;

            if (_throwCts == null || _throwCts.IsCancellationRequested)
            {
                _throwCts?.Dispose();
                _throwCts = new CancellationTokenSource();
            }

            _networkTr.Teleport(originPoint, originRot, Vector3.one);
            
            await UniTask.WaitUntil(() => !ItemRb.isKinematic, cancellationToken:_throwCts.Token);
            await Throwing(throwDir, originPoint, _throwCts.Token);
            await Falling(_throwCts.Token);
            
            IsThrowing = false;
        }
        
        private async UniTask Throwing(Vector3 dir, Vector3 origin, CancellationToken token)
        {
            if (ItemRb.isKinematic) return;
            
            ItemRb.linearVelocity = _data.ThrowForce * dir;

            while (ItemRb != null 
                   && !ItemRb.isKinematic
                   && !ShouldApplyDamping(origin) 
                   && HasEnoughVelocity(ItemRb.linearVelocity))
            {
                await UniTask.DelayFrame(5, cancellationToken:token);
            }
        }

        private async UniTask Falling(CancellationToken token)
        {
            if (ItemRb.isKinematic) return;

            while (ItemRb != null 
                   && !ItemRb.isKinematic
                   && ItemRb.linearVelocity.sqrMagnitude > 0.5f)
            {
                ItemRb.linearVelocity *= _data.DampingRatio;
                await UniTask.DelayFrame(5, cancellationToken:token);
            }
        }
        
        public bool HasEnoughVelocity(Vector3 velocity)
        {
            return velocity.sqrMagnitude > _data.ValidVelocityCutOff;
        }

        private bool ShouldApplyDamping(Vector3 origin)
        {
            var throwDist = (origin - ItemRb.position).sqrMagnitude;
            return throwDist > _data.DampingThreshold;
        }
        #endregion
        
        #region Prep 관련 메서드
        public float Prepare(int multiplier)
        {
            if (_curProgress >= _data.MaxProgress) return 1;
            
            _curProgress += Time.deltaTime * multiplier;
            return _curProgress / _data.MaxProgress;
        }

        public void OnPrepCompleted()
        {
            if (!HasAuthority) return;
            
            _curState = PrepState.WellDone;
            UpdateModelRpc(_curState);
        }

        public void OnOverCooked()
        {
            if (!HasAuthority) return;
            
            _curState = PrepState.OverDone;
            UpdateModelRpc(_curState);
        }
        #endregion
    }
}