using System.Collections.Generic;
using _Scripts._Wrapper;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Placable
{
    public class BoxTable : NetworkBehaviour, IPlacable
    {
        [SF] private bool spawnWithPlate;
        // Component
        private AttachableSlot _tableSlot;
        // Dependency
        private StageHub _stageHub;
        private PlacementBroker _placementBroker;
        // Caching
        private TagHandle _itemTag;
        // Property
        public Carriable PlacedItem { get; private set; }

        [Inject]
        private void Construct(
            StageHub stageHub,
            PlacementBroker placementBroker)
        {
            _stageHub = stageHub;
            _placementBroker = placementBroker;

            _itemTag = TagHandle.GetExistingTag("Item");
            
            _tableSlot = this.GetComponentInChildren<AttachableSlot>();
            _tableSlot.OnAttach += OnSlotAttached;
            _tableSlot.OnDetach += OnSlotDetached;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Ingredient ingredient) || !ingredient.IsThrowing) return;

            var result = _placementBroker.AcceptCase(ingredient, this);
            if (result.Reason is not null) Debug.LogWarning($"{result.Reason} [BoxTable{this.NetworkObjectId}_OnTrigger]");
        }

        #region NGO 관련 메서드
        public override void OnNetworkSpawn()
        {
            if (IsServer) NetworkManager.SceneManager.OnLoadEventCompleted += OnLevelLoaded;
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
            
            if (spawnWithPlate) AttachWithSpawn().Forget();
            
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLevelLoaded;
        }
        
        private void OnSlotAttached(Carriable item)
        {
            item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (IsServer) PlacedItem = item;
        }

        private void OnSlotDetached(Carriable item)
        {
            if (IsServer) PlacedItem = null;
        }
        #endregion

        #region Placable 관련 메서드
        public void Place(Carriable item)
        {
            if (item.IsCarrying) item.Detach();
            item.Attach(_tableSlot);
        }
        
        public bool CanPlace(Carriable item, out string rejectMessage)
        {
            rejectMessage = null;
            
            if (item == null || !item.IsSpawned)
            {
                rejectMessage = "Item이 null이거나 Spawn되지 않은 상태입니다.";
                return false;
            }
            
            return true;
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
        
        private async UniTaskVoid AttachWithSpawn()
        {
            var provider = _stageHub.FetchProvider<PlateProvider>();
            var plate = provider.GetPlate(this.transform.position);
            plate.NetObj.Spawn(true);
            
            await UniTask.Yield();
            
            if (!this.IsSpawned) return;
            
            plate.ClearHolder(true);
            this.Place(plate);
        }
        #endregion
    }
}