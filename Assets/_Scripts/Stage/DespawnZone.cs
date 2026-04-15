using UnityEngine;

namespace _Scripts.Stage
{
    public class DespawnZone : MonoBehaviour
    {
        private TagHandle _itemTag;
        private TagHandle _playerTag;

        private void Awake()
        {
            _itemTag = TagHandle.GetExistingTag("Item");
            _playerTag = TagHandle.GetExistingTag("Player");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_itemTag) && !other.CompareTag(_playerTag)) return;
            
            var despawnable = other.transform.parent.GetComponentInChildren<IDespawnable>();
            despawnable?.Despawn();
        }
    }
}