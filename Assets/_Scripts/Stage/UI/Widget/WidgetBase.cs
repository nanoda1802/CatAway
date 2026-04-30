using UnityEngine;
using VContainer;

namespace _Scripts.Stage.UI.Widget
{
    public class WidgetBase : MonoBehaviour
    {
        private RectTransform _canvasRectTr;
        private Camera _mainCam;

        protected RectTransform WidgetRectTr;
        
        [Inject]
        private void ConstructBase(RectTransform canvasRectTr, Camera mainCam)
        {
            _canvasRectTr = canvasRectTr;
            _mainCam = mainCam;

            WidgetRectTr = GetComponent<RectTransform>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            // 등장 Tween
        }

        public virtual void Hide()
        {
            // 퇴장 Tween
            gameObject.SetActive(false);
        }

        public virtual void UpdatePosition(Vector3 worldPos)
        {
            var screenPoint = _mainCam.WorldToScreenPoint(worldPos);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTr, 
                screenPoint, 
                null,
                out Vector2 localPoint
            );
            
            WidgetRectTr.anchoredPosition = localPoint;
        }
    }
}