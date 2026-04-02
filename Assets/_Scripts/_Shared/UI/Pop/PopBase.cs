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
        protected Canvas PopCanvas;
        protected CanvasGroup ViewGroup;
        protected PopPanel Bg;

        protected CanvasGroup PopGroup;
        
        protected Sequence CurSequence;

        private bool IsActive => PopCanvas.enabled;
        
        [Inject]
        private void ConstructBase(
            Canvas canvas,
            CanvasGroup canvasGroup,
            PopPanel bg,
            DisposableBagBuilder disposableBagBuilder,
            ISubscriber<PopUpMessage> popUpSub,
            ISubscriber<PopDownMessage> popDownSub)
        {
            PopGroup = this.GetComponent<CanvasGroup>();
            
            PopCanvas = canvas;
            ViewGroup = canvasGroup;
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
            if (msg.IsRequested(this) && !IsActive) PopUp();
        }

        private void HandlePopDownMessage(PopDownMessage msg)
        {
            PopDown();
        }

        protected virtual void PopUp()
        {
            if (CurSequence.isAlive) CurSequence.Complete();
            
            PopCanvas.enabled = true;
            ViewGroup.blocksRaycasts = true;
            
            PopGroup.alpha = 1;
            PopGroup.blocksRaycasts = true;
        }

        protected virtual void PopDown()
        {
            PopCanvas.enabled = false;
            ViewGroup.blocksRaycasts = false;
            
            PopGroup.alpha = 0;
            PopGroup.blocksRaycasts = false;
        }
    }
}