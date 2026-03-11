using _Scripts.Lobby.UI.Messages;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace _Scripts.Lobby.UI.Pop
{
    public class PopBase : MonoBehaviour
    {
        private CanvasGroup _popGroup;
        protected PopPanel Bg;
        
        protected readonly DisposableBagBuilder DisposableBag = MessagePipe.DisposableBag.CreateBuilder();
        
        [Inject]
        private void ConstructBase(
            CanvasGroup popUpGroup,
            PopPanel bg,
            ISubscriber<PopUpMessage> popUpSub,
            ISubscriber<PopDownMessage> popDownSub)
        {
            _popGroup = popUpGroup;
            Bg = bg;

            popUpSub.Subscribe(msg =>
            {
                if (msg.IsRequested(this)) this.PopUp();
            }).AddTo(DisposableBag);
            
            popDownSub
                .Subscribe(msg => PopDown())
                .AddTo(DisposableBag);
        }

        private void OnDestroy()
        {
            DisposableBag?.Build().Dispose();
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