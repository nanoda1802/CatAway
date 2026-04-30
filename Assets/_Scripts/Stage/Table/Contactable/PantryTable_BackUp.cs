using _Scripts.Stage._Data.Item;
using _Scripts.Stage._Enums;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Contactable
{
    public class PantryTable_BackUp : NetworkBehaviour, IContactable
    {
        // Data
        [SF] private IngredientType presetType;
        // Dependency
        private StageHub _stageHub;
        // Component
        [SF] private MeshFilter sampleMeshFilter;
        [SF] private Transform sampleTransform;
        
        private readonly NetworkVariable<IngredientType> _sharedIngredientType = new();

        public IngredientType PresetType => presetType;
        
        [Inject]
        private void Construct(StageHub stageHub)
        {
            _stageHub = stageHub;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) _sharedIngredientType.Value = presetType;

            _sharedIngredientType.OnValueChanged += OnTypeChanged;

            base.OnNetworkSpawn();
        }

        protected override void OnNetworkPostSpawn()
        {
            UpdateSampleModel(_sharedIngredientType.Value);
            
            base.OnNetworkPostSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _sharedIngredientType.OnValueChanged -= OnTypeChanged;
            
            base.OnNetworkDespawn();
        }

        public void SetAs(IngredientType type)
        {
            _sharedIngredientType.Value = type;
        }

        private void UpdateSampleModel(IngredientType type)
        {
            var provider = _stageHub.FetchProvider<IngredientProvider>();
            (Mesh mesh, Vector3 scale) = provider.GetModelInfo(type);
            
            sampleMeshFilter.sharedMesh = mesh;
            sampleTransform.localScale = scale;
        }

        private void OnTypeChanged(IngredientType prev, IngredientType cur)
        {
            if (prev == cur) return;
            UpdateSampleModel(cur);
        }

        #region Contactable 관련 메서드
        public bool TryContact(Carriable item, out string failMessage)
        {
            failMessage = null;
            
            if (item is not null)
            {
                failMessage = "이미 Carry 중인 아이템이 있습니다.";
                return false;
            }
            
            return true;
        }

        public void RespondTo(CarrierBehaviour carrier)
        {
            AttachWithSpawn(carrier).Forget();
        }

        private async UniTaskVoid AttachWithSpawn(CarrierBehaviour carrier)
        {
            var provider = _stageHub.FetchProvider<IngredientProvider>();
            var ingredient = provider.GetIngredient(presetType, transform.position);
            
            NetworkManager.PrefabHandler.SetInstantiationData(ingredient.NetObj,new IngredientTypePacket(presetType));
            ingredient.NetObj.Spawn(true);
            
            await UniTask.Yield();
            
            if (!this.IsSpawned) return;
            
            carrier.Pick(ingredient);
        }
        #endregion
    }
}