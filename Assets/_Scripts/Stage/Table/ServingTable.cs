using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class ServingTable : NetworkBehaviour, IPlacable
    {
        private IPlacable _plateReturnTable;
        
        private TagHandle _itemTag;

        public Carriable PlacedItem => null;

        private void Awake() // [임시]
        {
            _plateReturnTable = FindFirstObjectByType<PlateReturnTable>();
        }

        public bool TryPlace(Carriable carriable)
        {
            if (carriable == null || !carriable.IsSpawned) return false;
            if (carriable.Type != CarriableType.Plate) return false;
            if (!carriable.NetworkObject.TryGetComponent(out Plate plate)) return false;
            if (!plate.IsReady || !plate.HasIngredient) return false;
            
            // [추가] OrderPresenter에 platingList를 제출
            
            plate.ClearHolder();
            
            if (carriable.IsAttach) carriable.Detach();
            _plateReturnTable.TryPlace(carriable);
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable)
        {
            carriable = null;
            return false;
        }
    }
}