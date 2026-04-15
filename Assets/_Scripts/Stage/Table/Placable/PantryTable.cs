using System.Collections.Generic;
using _Scripts._Wrapper;
using _Scripts.Stage._Data.Item;
using _Scripts.Stage._Enums;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Placable
{
    public class PantryTable : NetworkBehaviour, IPlacable
    {
        [SF] private IngredientType presetType;
        
        private AttachableSlot _tableSlot;
        private StageHub _stageHub;
        
        private readonly NetworkVariable<IngredientType> _sharedIngredientType = new();
        
        private IngredientType IngredientType => _sharedIngredientType.Value;
        
        public Carriable PlacedItem { get; private set; }
        
        [Inject]
        private void Construct(StageHub stageHub)
        {
            _stageHub = stageHub;
            
            _tableSlot = this.GetComponentInChildren<AttachableSlot>();
            _tableSlot.OnAttach += OnSlotAttached;
            _tableSlot.OnDetach += OnSlotDetached;
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.SceneManager.OnLoadEventCompleted += OnLevelLoaded;
                _sharedIngredientType.Value = presetType;
            }
            base.OnNetworkSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            _tableSlot.OnAttach -= OnSlotAttached;
            _tableSlot.OnDetach -= OnSlotDetached;
            
            base.OnNetworkPreDespawn();
        }

        private void OnLevelLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!NetworkManager.IsServer) return;
            if (!sceneName.StartsWith("Level")) return;
            
            AttachWithSpawn().Forget();
            
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLevelLoaded;
        }
        
        private void OnSlotAttached(Carriable item)
        {
            item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (IsServer) PlacedItem = item;
        }

        private void OnSlotDetached(Carriable item)
        {
            if (!IsServer) return;
            PlacedItem = null;
            AttachWithSpawn().Forget();
        }
        
        private async UniTaskVoid AttachWithSpawn()
        {
            var provider = _stageHub.FetchProvider<IngredientProvider>();
            var ingredient = provider.GetIngredient(IngredientType, this.transform.position);
            
            NetworkManager.PrefabHandler.SetInstantiationData(ingredient.NetObj,new IngredientTypePacket(IngredientType));
            ingredient.NetObj.Spawn(true);
            
            await UniTask.Yield();
            
            if (!this.IsSpawned) return;
            
            this.Place(ingredient);
        }
        
        public void Place(Carriable item)
        {
            if (item == null || !item.IsSpawned) return;
            if (item.IsCarrying) item.Detach();
            item.Attach(_tableSlot);
        }

        public bool CanPlace(Carriable item, out string rejectMessage)
        {
            rejectMessage = "Pantry는 place할 수 없는 테이블입니다.";
            return false;
        }

        public bool CanDisPlace(out string rejectMessage)
        {
            rejectMessage = null;
            
            if (PlacedItem == null || !PlacedItem.IsSpawned)
            {
                rejectMessage = "PlacedItem이 null이거나 Spawn되지 않은 상태입니다.";
                return false;
            }
            
            return true;
        }
    }
}