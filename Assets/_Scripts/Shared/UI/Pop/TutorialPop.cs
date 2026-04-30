using _Scripts._Helper;
using PrimeTween;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Shared.UI.Pop
{
    public class TutorialPop : PopBase
    {
        [Header("[ Components ]")]
        [SF] private RectTransform rectTr;
        
        [Header("[ Tween Settings ]")]
        [SF] private TweenSettings<float> popUpPosSettings;
        [SF] private TweenSettings<float> popUpAlphaSettings;
        [SF] private TweenSettings<float> popDownPosSettings;
        [SF] private TweenSettings<float> popDownAlphaSettings;
        
        private TweenHandler _tweenHandler;

        [Inject]
        private void Construct(TweenHandler tweenHandler)
        {
            _tweenHandler = tweenHandler;
        }

        protected override void PopUp()
        {
            base.PopUp();
            CurSequence = _tweenHandler.AnchorPosY(ViewGroup, rectTr, popUpAlphaSettings, popUpPosSettings, OnPopUpCompleted);
        }

        protected override void PopDown()
        {
            if (CurSequence.isAlive) CurSequence.Complete();
            CurSequence = _tweenHandler.AnchorPosY(ViewGroup, rectTr, popDownAlphaSettings, popDownPosSettings, OnPopDownCompleted);
            
            Bg.OnClick -= PopDown;
        }
        
        private void OnPopUpCompleted()
        {
            Bg.OnClick += PopDown;
        }

        private void OnPopDownCompleted()
        {
            base.PopDown();
        }
    }
}