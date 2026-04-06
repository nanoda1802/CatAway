using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage
{
    public class VirtualButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SF] private Image iconImg;
        
        private Color _defaultColor;
        private Color _pressedColor;

        private void Awake()
        {
            _defaultColor = iconImg.color;
            _pressedColor = _defaultColor + new Color(50, 50, 50, 0);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            iconImg.color = _defaultColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            iconImg.color = _pressedColor;
        }
    }
}