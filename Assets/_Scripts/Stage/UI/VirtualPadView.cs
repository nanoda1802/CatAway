using UnityEngine;

namespace _Scripts.Stage.UI
{
    public class VirtualPadView : MonoBehaviour
    {
        private void Awake()
        {
            bool isMobile = Application.isMobilePlatform;
            this.gameObject.SetActive(isMobile);
        }
    }
}