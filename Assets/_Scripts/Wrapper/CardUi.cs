using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Wrapper
{
    public class CardUi : MonoBehaviour
    {
        public Image CardImg { get; private set; }
        public RectTransform RectTr { get; private set; }
        
        private void OnEnable()
        {
            CardImg ??= GetComponent<Image>();
            RectTr ??= GetComponent<RectTransform>();
        }
    }
}