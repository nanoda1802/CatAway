using _Scripts._Helper;
using _Scripts._Shared.Data;
using _Scripts.Messages;
using MessagePipe;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.UI.Pop
{
    public class CustomizePop : PopBase
    {
        [Header("[ Components ]")]
        [SF] private RectTransform rectTr;
        [SF] private Button[] colorButtons;
        [SF] private Image[] colorIcons;
        
        [Header("[ Tween Settings ]")]
        [SF] private TweenSettings<float> popUpPosSettings;
        [SF] private TweenSettings<float> popUpAlphaSettings;
        [SF] private TweenSettings<float> popDownPosSettings;
        [SF] private TweenSettings<float> popDownAlphaSettings;
        
        private TweenHandler _tweenHandler;
        private AvatarData _avatarData;
        private IPublisher<AvatarMessage> _avatarPub;

        [Inject]
        private void Construct(
            TweenHandler tweenHandler,
            AvatarData avatarData,
            IPublisher<AvatarMessage> avatarPub)
        {
            _tweenHandler = tweenHandler;
            _avatarData = avatarData;
            _avatarPub = avatarPub;
            
            InitColorIcons();
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
        }

        private void OnPopUpCompleted()
        {
            Bg.OnClick += PopDown;

            foreach (var btn in colorButtons)
            {
                var siblingIdx = btn.transform.GetSiblingIndex();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnClickColorIcon(siblingIdx));
            }
        }

        private void OnPopDownCompleted()
        {
            base.PopDown();
            
            Bg.OnClick -= PopDown;
            
            foreach (var btn in colorButtons)
            {
                btn.onClick.RemoveAllListeners();
            }
        }

        private void InitColorIcons()
        {
            for (int i = 0; i < colorIcons.Length; i++)
            {
                colorIcons[i].sprite = _avatarData.GetCoatColorSprite(i);
            }
        }
        
        private void OnClickColorIcon(int idx)
        {
            var msg = new AvatarMessage(idx);
            _avatarPub.Publish(msg);
        }
    }
}