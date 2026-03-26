using _Scripts.Messages;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace _Scripts._Shared.UI.Pop
{
    public class PopBase : MonoBehaviour
    {
        private CanvasGroup _popGroup;
        protected PopPanel Bg;
        
        [Inject]
        private void ConstructBase(
            CanvasGroup popUpGroup,
            PopPanel bg,
            DisposableBagBuilder disposableBagBuilder,
            ISubscriber<PopUpMessage> popUpSub,
            ISubscriber<PopDownMessage> popDownSub)
        {
            _popGroup = popUpGroup;
            Bg = bg;

            popUpSub.Subscribe(msg =>
                {
                    if (!msg.IsRequested(this)) return; 
                    this.PopUp();
                }).AddTo(disposableBagBuilder);
            
            popDownSub.Subscribe(msg => 
                {
                    if (!this.isActiveAndEnabled) return;
                    PopDown();
                })
                .AddTo(disposableBagBuilder);
        }

        protected virtual void PopUp()
        {
            _popGroup.alpha = 1;
            _popGroup.blocksRaycasts = true;
            
            this.gameObject.SetActive(true);
        }

        protected virtual void PopDown()
        {
            _popGroup.alpha = 0;
            _popGroup.blocksRaycasts = false;
            
            this.gameObject.SetActive(false);
        }
    }
}