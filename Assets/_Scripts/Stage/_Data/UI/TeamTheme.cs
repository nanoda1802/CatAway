using System;
using UnityEngine;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage._Data.UI
{
    [Serializable]
    public struct TeamTheme
    {
        [SF] private Color imageColor;
        [SF] private Color textColor;
        [SF] private Image.OriginHorizontal fillOrigin;
        
        public Color ImageColor => imageColor;
        public Color TextColor => textColor;
        public Image.OriginHorizontal FillOrigin => fillOrigin;
    }
}