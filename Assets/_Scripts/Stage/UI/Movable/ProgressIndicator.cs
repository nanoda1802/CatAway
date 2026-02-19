using System;
using UnityEngine;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Movable
{
    public class ProgressIndicator : MonoBehaviour
    {
        [SF] private Image fillBarImg;

        private RectTransform _canvasRectTr;
        private RectTransform _rectTr;
        private Camera _mainCam;

        private void Awake() // [임시]
        {
            _mainCam = Camera.main;
            _canvasRectTr = transform.parent.GetComponent<RectTransform>();
            _rectTr = GetComponent<RectTransform>();
        }

        public void SetPos(Vector3 worldPos)
        {
            var screenPoint = _mainCam.WorldToScreenPoint(worldPos);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTr, 
                screenPoint, 
                null,
                out Vector2 localPoint
            );
            
            _rectTr.anchoredPosition = localPoint;
        }

        public void UpdateProgress(float prevValue, float newValue)
        {
            if (prevValue >= newValue) return;
            fillBarImg.fillAmount = Mathf.Lerp(0,1,newValue);
        }

        public void ResetProgress()
        {
            fillBarImg.fillAmount = 0;
        }
    }
}