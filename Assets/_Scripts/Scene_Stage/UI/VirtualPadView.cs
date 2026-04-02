using System;
using UnityEngine;

namespace _Scripts.Scene_Stage
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