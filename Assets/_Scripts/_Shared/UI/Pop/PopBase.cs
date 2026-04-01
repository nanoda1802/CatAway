using System;
using _Scripts.Messages;
using MessagePipe;
using PrimeTween;
using UnityEngine;
using VContainer;

namespace _Scripts._Shared.UI.Pop
{
    public class PopBase : MonoBehaviour
    {
        protected CanvasGroup PopGroup;
        protected PopPanel Bg;
        
        protected Sequence CurSequence;
        
        [Inject]
        private void ConstructBase(
            CanvasGroup popUpGroup,
            PopPanel bg,
            DisposableBagBuilder disposableBagBuilder,
            ISubscriber<PopUpMessage> popUpSub,
            ISubscriber<PopDownMessage> popDownSub)
        {
            PopGroup = popUpGroup;
            Bg = bg;

            popUpSub
                .Subscribe(HandlePopUpMessage)
                .AddTo(disposableBagBuilder);
            
            popDownSub
                .Subscribe(HandlePopDownMessage)
                .AddTo(disposableBagBuilder);
        }

        private void OnDestroy()
        {
            if (CurSequence.isAlive) CurSequence.Complete();
        }

        private void HandlePopUpMessage(PopUpMessage msg)
        {
            if (msg.IsRequested(this) && !isActiveAndEnabled) PopUp();
        }

        private void HandlePopDownMessage(PopDownMessage msg)
        {
            PopDown();
        }

        protected virtual void PopUp()
        {
            if (CurSequence.isAlive) CurSequence.Complete();
            
            PopGroup.blocksRaycasts = true;
            
            this.gameObject.SetActive(true);
        }

        protected virtual void PopDown()
        {
            PopGroup.blocksRaycasts = false;
            
            this.gameObject.SetActive(false);
        }
    }
}