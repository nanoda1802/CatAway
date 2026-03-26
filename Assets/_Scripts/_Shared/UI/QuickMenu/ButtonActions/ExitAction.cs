using _Scripts._Shared.Enums;
using UnityEngine;

namespace _Scripts._Shared.UI.QuickMenu.ButtonActions
{
    public class ExitAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Exit;

        public ExitAction()
        {
            // 주입
        }

        public void OnClick()
        {
            Debug.Log("Exit Game");
        }
    }
}