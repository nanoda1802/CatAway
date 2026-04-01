using System;
using UnityEngine;
using UnityEngine.EventSystems;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.UI.Pop
{
    public class PopPanel : MonoBehaviour, IPointerClickHandler
    {
        public event Action OnClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsBlocked(eventData.pointerCurrentRaycast.gameObject)) return;
            OnClick?.Invoke();
            OnClick = null;
        }

        private bool IsBlocked(GameObject target)
        {
            return target != this.gameObject;
        }
    }
}