using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class ServingTable : NetworkBehaviour, IPlacable
    {
        /* 기타 */
        private TableHub _tableHub;
        /* 프로퍼티 */
        public Carriable PlacedItem => null;

        [Inject]
        private void Construct(TableHub tableHub)
        {
            _tableHub = tableHub;
        }

        public bool TryPlace(Carriable item)
        {
            if (item == null || !item.IsSpawned) return false;
            if (item.Type != CarriableType.Plate) return false;
            if (!item.NetworkObject.TryGetComponent(out Plate plate)) return false;
            if (!plate.IsReady || !plate.HasIngredient) return false;
            
            // [추가] OrderPresenter에 platingList를 제출
            
            plate.ClearHolder();
            _tableHub.Fetch<PlateReturnTable>()?.TryPlace(item);
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable displacedItem)
        {
            displacedItem = null;
            return false;
        }
    }
}