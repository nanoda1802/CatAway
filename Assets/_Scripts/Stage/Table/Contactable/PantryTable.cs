using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Contactable
{
    public class PantryTable : NetworkBehaviour, IContactable
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

        protected override void OnNetworkPostSpawn()
        {
            presetType = _sharedIngredientType.Value;
            
            var provider = _stageHub.FetchProvider<IngredientProvider>();
            (Mesh mesh, Vector3 scale) = provider.GetModelInfo(_sharedIngredientType.Value);
            
            sampleMeshFilter.sharedMesh = mesh;
            sampleTransform.localScale = scale;
            
            base.OnNetworkPostSpawn();
        }

        public void SetAs(IngredientType type)
        {
            _sharedIngredientType.Value = type;
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
            NetworkManager.PrefabHandler.SetInstantiationData(ingredient.NetworkObject,new IngredientTypePacket(presetType));
            ingredient.NetworkObject.Spawn();
            
            await UniTask.Yield();
            carrier.Pick(ingredient);
        }
        #endregion
    }
}