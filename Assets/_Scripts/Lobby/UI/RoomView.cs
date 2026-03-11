using System;
using System.Collections.Generic;
using System.Threading;
using _Scripts.Lobby.UI.Messages;
using _Scripts.Lobby.UI.Messages.Member;
using _Scripts.Lobby.UI.Messages.Room;
using _Scripts.Lobby.UI.Room;
using _Scripts.Stage;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI
{
    public class RoomView : MonoBehaviour, IView
    {
        [Header("[ Components ]")]
        [SF] private CanvasGroup canvasGroup;
        [SF] private Button changeModeBtn;
        [SF] private Button confirmBtn;
        [SF] private TextMeshProUGUI confirmBtnTxt;
        
        [Header("[ View Elements ]")]
        [SF] private SectionBase[] sections;
        [SF] private QuickMenuType quickMenuType  = QuickMenuType.Customize | QuickMenuType.Tutorial | QuickMenuType.Setting | QuickMenuType.Leave;
        
        // Dependency
        private MemberCardProvider _cardProvider;
        // Caching
        private readonly Dictionary<ulong, MemberCard> _activeCards = new();
        private readonly List<UniTask> _sectionTasks = new();
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        // Property
        public QuickMenuType RequiredQuickMenu => quickMenuType;

        [Inject]
        private void Construct(
            MemberCardProvider memberCardProvider,
            ISubscriber<InitRoomMessage> initRoomSub,
            ISubscriber<SwitchReadyRespond> readyResSub,
            ISubscriber<ShowMemberCardMessage> showCardSub,
            ISubscriber<HideMemberCardMessage> hideCardSub)
        {
            if (_cardProvider != null) return; // [과제] RectTr도 등록하느라 주입이 두 번 되는 모양인데...
            
            _cardProvider = memberCardProvider;

            initRoomSub
                .Subscribe(InitSections)
                .AddTo(_disposableBagBuilder);

            readyResSub
                .Subscribe(UpdateCardReadyState)
                .AddTo(_disposableBagBuilder);
            
            showCardSub
                .Subscribe(AddMemberCard)
                .AddTo(_disposableBagBuilder);
            
            hideCardSub
                .Subscribe(RemoveMemberCard)
                .AddTo(_disposableBagBuilder);
        }

        private void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        #region IView 구현 메서드
        public async UniTask Activate(CancellationToken token)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;

            await ShowSections(token);
        }

        public async UniTask Deactivate(CancellationToken token)
        {
            await HideSections(token);
            
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;

            foreach (var activeCard in _activeCards.Values)
            {
                activeCard.Hide();
                _cardProvider.ReleaseCard(activeCard);
            }
            
            _activeCards.Clear();
        }
        #endregion

        #region Section 관련 메서드
        private void InitSections(InitRoomMessage msg)
        {
            foreach (var section in sections)
            {
                switch (section)
                {
                    case CodeSection codeSection:
                        codeSection.InitElements(msg.Code);
                        break;
                    case SelectionSection selectionSection:
                        selectionSection.InitElements(msg.Mode, msg.IsHostPlayer);
                        break;
                    case ButtonSection buttonSection:
                        buttonSection.InitElements(msg.Mode, msg.IsHostPlayer);
                        break;
                    default:
                        break;
                }
            }
        }

        private async UniTask ShowSections(CancellationToken token)
        {
            _sectionTasks.Clear();
            
            foreach (var section in sections)
            {
                _sectionTasks.Add(section.Show(token));
            }
            
            await UniTask.WhenAll(_sectionTasks);
        }

        private async UniTask HideSections(CancellationToken token)
        {
            _sectionTasks.Clear();
            
            foreach (var section in sections)
            {
                _sectionTasks.Add(section.Hide(token));
            }
            
            await UniTask.WhenAll(_sectionTasks);
        }
        #endregion

        #region MemberCard 관련 메서드
        private void AddMemberCard(ShowMemberCardMessage msg)
        {
            var newCard = _cardProvider.GetCard(msg.SpawnPoint)
                .SetIcon(msg.MemberType)
                .SetName($"Player{msg.MemberId}");

            _activeCards[msg.MemberId] = newCard;
            
            newCard.Show();
        }

        private void RemoveMemberCard(HideMemberCardMessage msg)
        {
            if (!_activeCards.Remove(msg.MemberId, out var targetCard)) return;
            
            targetCard.Hide();
            
            _cardProvider.ReleaseCard(targetCard);
        }

        private void UpdateCardReadyState(SwitchReadyRespond res)
        {
            if (!_activeCards.TryGetValue(res.MemberId, out var targetCard)) return;
            targetCard.SwitchReadyIcon(res.IsReady);
        }

        private void UpdateCardPos()
        {
        }
        #endregion
        
    }
}