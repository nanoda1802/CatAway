using UnityEngine;
using UnityEngine.UI;

namespace _Scripts._Wrapper
{
    public class Thumbnail : MonoBehaviour
    {
        public Image Image { get; private set; }
        public RectTransform RectTr { get; private set; }
        
        private void OnEnable()
        {
            Image ??= GetComponent<Image>();
            RectTr ??= GetComponent<RectTransform>();
        }
    }
}