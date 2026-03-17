using _Scripts.Messages;
using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Pop
{
    public class CustomizePop : PopBase
    {
        [SF] private Button[] colorButtons;
        [SF] private Image[] colorIcons;
        
        private AvatarData _avatarData;
        private IPublisher<AvatarMessage> _avatarPub;

        [Inject]
        private void Construct(
            AvatarData avatarData,
            IPublisher<AvatarMessage> avatarPub)
        {
            _avatarData = avatarData;
            _avatarPub = avatarPub;
            
            InitColorIcons();
        }

        protected override void PopUp()
        {
            base.PopUp();
            
            Bg.OnClick += PopDown;
            Bg.OnSwipeDown += PopDown;

            foreach (var btn in colorButtons)
            {
                var siblingIdx = btn.transform.GetSiblingIndex();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnClickColorIcon(siblingIdx));
            }
        }

        protected override void PopDown()
        {
            foreach (var btn in colorButtons)
            {
                btn.onClick.RemoveAllListeners();
            }
            
            Bg.OnClick -= PopDown;
            Bg.OnSwipeDown -= PopDown;
            
            base.PopDown();
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