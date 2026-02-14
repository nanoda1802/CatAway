using System;
using _Scripts.Stage.Item;
using UnityEngine;

namespace _Scripts.Stage.Player
{
    public class DetectStatus
    {
        private readonly Transform _detectPoint;
        private readonly PlayerData _data;
        
        private readonly Collider[] _detectedItems;
        
        public DetectStatus(Transform detectPoint, PlayerData data)
        {
            _detectPoint = detectPoint;
            _data = data;
            _detectedItems = new Collider[data.MaxDetectionCount];
        }
        
        public bool DetectItem(out Carriable closest)
        {
            closest = null;
            
            Vector3 offset = _detectPoint.forward  * _data.OverlapBoxOffset;
            int hitCount = Physics.OverlapBoxNonAlloc(_detectPoint.position + offset, _data.OverlapBoxSize, _detectedItems, _detectPoint.rotation,_data.ItemLayer);

            if (hitCount <= 0) return false;
            
            float minDist = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                if (_detectedItems[i] is null) continue;
                if (_detectedItems[i].transform.TryGetComponent(out Carriable item)) continue;
                
                float dist = (_detectPoint.position - _detectedItems[i].transform.position).sqrMagnitude;
                
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = item;
                }
            }
            
            return closest is not null;
        }
    
        public bool DetectTable(out GameObject table)
        {
            bool isHit = Physics.Raycast(_detectPoint.position, _detectPoint.forward, out RaycastHit hit, _data.RayDistance,
                _data.TableLayer);
            
            table = isHit ? hit.collider.gameObject : null;
            
            return isHit;
        }
    }
}