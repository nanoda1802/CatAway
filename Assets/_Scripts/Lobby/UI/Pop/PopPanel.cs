using System;
using UnityEngine;
using UnityEngine.EventSystems;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Pop
{
    public class PopPanel : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public event Action OnClick;
        public event Action OnSwipeDown;
        
        [SF] private float validSwipeDistanceRatio = 0.2f;
        [SF] private float swipeDotThreshold = 0.8f;
        
        private Vector2 _dragStartPos;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsBlocked(eventData.pointerCurrentRaycast.gameObject)) return;
            OnClick?.Invoke();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsBlocked(eventData.pointerCurrentRaycast.gameObject)) return;
            if (OnSwipeDown == null) return;
            _dragStartPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // [메모] 얘를 구현해야 Begin과 End가 작동
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (IsBlocked(eventData.pointerCurrentRaycast.gameObject)) return;
            if (OnSwipeDown == null) return;
            if (!IsDownSwipe(eventData.position)) return;
            
            OnSwipeDown?.Invoke();
        }
        
        private bool IsDownSwipe(Vector2 endPos)
        {
            Vector2 diff = endPos - _dragStartPos;
        
            if (diff.magnitude < Screen.height * validSwipeDistanceRatio) return false;

            Vector2 swipeDir = diff.normalized;
            
            float dot = Vector2.Dot(swipeDir, Vector2.down);
            
            return dot >= swipeDotThreshold;
        }

        private bool IsBlocked(GameObject target)
        {
            return target != this.gameObject;
        }
    }
}