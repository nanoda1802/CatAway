using System.Collections.Generic;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class PlateRackTable : NetworkBehaviour, IPlacable
    {
        /* 데이터 */
        [SF] private float plateOffsetY = 0.12f;
        /* 컴포넌트 */
        private AttachableNode _pivot;
        /* 캐싱 */
        private readonly Stack<Carriable> _washedPlates = new();
        /* 프로퍼티 */
        public Carriable PlacedItem => _washedPlates.TryPeek(out Carriable plate) ? plate : null;
        
        [Inject]
        private void Construct(IBufferedPublisher<PlateRackTable> pub)
        {
            pub.Publish(this);
            
            _pivot = GetComponentInChildren<AttachableNode>();
        }
        
        [Rpc(SendTo.Everyone)]
        private void UpdatePositionRpc(NetworkBehaviourReference carriableRef, Vector3 localPos)
        {
            if (!carriableRef.TryGet(out Carriable carriable)) return;
            carriable.transform.localPosition = localPos;
        }
        
        public bool TryPlace(Carriable item)
        {
            if (item == null || !item.IsSpawned) return false; 
            if (item.Type != CarriableType.Plate) return false;
            if (!item.NetworkObject.TryGetComponent(out Plate plate) || !plate.IsReady) return false;
            
            _washedPlates.Push(item);
            
            item.AttachTo(_pivot);

            var newPlatePos = (_washedPlates.Count - 1) * plateOffsetY * Vector3.up;
            UpdatePositionRpc(item, newPlatePos);
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable displacedItem)
        {
            if (!_washedPlates.TryPop(out displacedItem)) return false;
            displacedItem?.Detach();
            
            return displacedItem is not null;
        }
    }
}