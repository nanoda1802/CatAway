using System.Collections.Generic;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class PlateReturnTable : NetworkBehaviour, IPlacable
    {
        [SF] private AttachableNode pivot;
        [SF] private float plateOffsetY = 0.07f;
        
        private readonly Stack<Carriable> _returnedPlates = new();
        
        public Carriable PlacedItem => _returnedPlates.TryPeek(out Carriable plate) ? plate : null;
        
        public bool TryPlace(Carriable carriable)
        {
            if (carriable == null || !carriable.IsSpawned) return false;
            if (carriable.Type != CarriableType.Plate) return false;
            if (!carriable.NetworkObject.TryGetComponent(out Plate plate) || plate.IsReady) return false;
            
            _returnedPlates.Push(carriable);
            
            if (carriable.IsAttach) carriable.Detach();
            carriable.Attach(pivot);

            var newPlatePos = (_returnedPlates.Count - 1) * plateOffsetY * Vector3.up;
            SetPlatePositionRpc(carriable, newPlatePos);
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable)
        {
            if (!_returnedPlates.TryPop(out carriable)) return false;
            carriable?.Detach();
            
            return carriable is not null;
        }
        
        [Rpc(SendTo.Everyone)]
        private void SetPlatePositionRpc(NetworkBehaviourReference carriableRef, Vector3 localPos)
        {
            if (!carriableRef.TryGet(out Carriable carriable)) return;
            carriable.transform.localPosition = localPos;
        }
    }
}