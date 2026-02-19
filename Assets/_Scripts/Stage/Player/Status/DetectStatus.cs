using _Scripts.Stage.Item;
using _Scripts.Stage.Table;
using UnityEngine;

namespace _Scripts.Stage.Player.Status
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

            // DrawDebugBox(_detectPoint.position + offset, _data.OverlapBoxSize, _detectPoint.rotation, Color.cyan);
            
            if (hitCount <= 0) return false;
            
            float minDist = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                if (_detectedItems[i] is null) continue;
                if (!_detectedItems[i].TryGetComponent<Carriable>(out Carriable item)) continue;
                
                float dist = (_detectPoint.position - _detectedItems[i].transform.position).sqrMagnitude;
                
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = item;
                }
            }
            
            return closest != null;
        }
    
        private void DrawDebugBox(Vector3 center, Vector3 halfSize, Quaternion rotation, Color color)
        {
            // 박스의 8개 로컬 정점 정의
            Vector3[] points = new Vector3[8];
            points[0] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            points[1] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            points[2] = new Vector3(halfSize.x, -halfSize.y,  halfSize.z);
            points[3] = new Vector3(-halfSize.x, -halfSize.y,  halfSize.z);
            points[4] = new Vector3(-halfSize.x,  halfSize.y, -halfSize.z);
            points[5] = new Vector3(halfSize.x,  halfSize.y, -halfSize.z);
            points[6] = new Vector3(halfSize.x,  halfSize.y,  halfSize.z);
            points[7] = new Vector3(-halfSize.x,  halfSize.y,  halfSize.z);

            // 회전 및 위치 적용 (로컬 -> 월드 변환)
            for (int i = 0; i < 8; i++)
            {
                points[i] = rotation * points[i] + center;
            }

            // 선 그리기 (바닥면, 윗면, 기둥 순)
            for (int i = 0; i < 4; i++)
            {
                Debug.DrawLine(points[i], points[(i + 1) % 4], color); // 바닥
                Debug.DrawLine(points[i + 4], points[((i + 1) % 4) + 4], color); // 천장
                Debug.DrawLine(points[i], points[i + 4], color); // 기둥
            }
        }
        
        public bool DetectTable(out GameObject table)
        {
            bool isHit = Physics.Raycast(_detectPoint.position, _detectPoint.forward, out RaycastHit hit, _data.RayDistance,
                _data.TableLayer);
            
            table = isHit ? hit.collider.gameObject : null;
            
            return isHit && table is not null;
        }
    }
}