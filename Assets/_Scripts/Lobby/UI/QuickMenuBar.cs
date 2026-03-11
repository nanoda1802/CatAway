using System;
using _Scripts.Lobby.UI.Messages;
using _Scripts.Lobby.UI.Messages.Member;
using _Scripts.Lobby.UI.Pop;
using AYellowpaper.SerializedCollections;
using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI
{
    public class QuickMenuBar : MonoBehaviour
    {
        [SF] private SerializedDictionary<QuickMenuType, Button> buttons;

        private IPublisher<DialogMessage> _dialogPub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<SwitchReadyRequest> _switchReadyPub;
        
        [Inject]
        private void Construct(
            IPublisher<DialogMessage> dialogPub,
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<SwitchReadyRequest> switchReadyPub)
        {
            _dialogPub = dialogPub;
            _popUpPub = popUpPub;
            _switchReadyPub = switchReadyPub;
        }

        private void OnEnable()
        {
            buttons[QuickMenuType.Exit]?.onClick.AddListener(OnClickExit);
            buttons[QuickMenuType.Leave]?.onClick.AddListener(OnClickLeave);
            buttons[QuickMenuType.Setting]?.onClick.AddListener(OnClickSetting);
            buttons[QuickMenuType.Customize]?.onClick.AddListener(OnClickCustomize);
            buttons[QuickMenuType.Tutorial]?.onClick.AddListener(OnClickTutorial);
            
            RefreshBtnGroup(QuickMenuType.Customize | QuickMenuType.Tutorial | QuickMenuType.Setting | QuickMenuType.Exit);
        }

        private void OnDisable()
        {
            foreach (var btn in buttons.Values)
            {
                btn.onClick.RemoveAllListeners();
            }
        }

        public void RefreshBtnGroup(QuickMenuType requiredType)
        {
            foreach (var pair in buttons)
            {
                var btn = pair.Value;
                var required = requiredType.HasFlag(pair.Key);
                btn.gameObject.SetActive(required);
            }
        }

        #region OnClick 메서드
        private void OnClickExit()
        {
            Debug.Log("Exit Game");
        }

        private void OnClickLeave()
        {
            _dialogPub.Publish(new DialogMessage(
                "Leave Room",
                "Return to Title?",
                string.Empty, 
                DialogButtonType.Confirm | DialogButtonType.Cancel
            ));
            
            _popUpPub.Publish(new PopUpMessage(typeof(DialogPop)));
        }

        private void OnClickSetting()
        {
            Debug.Log("Pop Up Setting");
            _switchReadyPub.Publish(new SwitchReadyRequest(true));
        }

        private void OnClickTutorial()
        {
            _popUpPub.Publish(new PopUpMessage(typeof(TutorialPop)));
            _switchReadyPub.Publish(new SwitchReadyRequest(true));
        }
        
        private void OnClickCustomize()
        {
            _popUpPub.Publish(new PopUpMessage(typeof(CustomizePop)));
            _switchReadyPub.Publish(new SwitchReadyRequest(true));
        }
        #endregion
    }
}