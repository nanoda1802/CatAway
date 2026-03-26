using System.Collections.Generic;
using System.Threading;
using _Scripts._Messages.Room;
using _Scripts._Messages.Shared;
using _Scripts.Messages.Room;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Room.UI
{
    public class RoomView : MonoBehaviour
    {
        [Header("[ View Elements ]")]
        [SF] private SectionBase[] sections;
        
        // Dependency
        private RoomMemberCardProvider _cardProvider;
        // Caching
        private readonly Dictionary<ulong, RoomMemberCard> _activeCards = new();
        private readonly List<UniTask> _sectionTasks = new();
        private CancellationTokenSource _cts;

        [Inject]
        private void Construct(
            RoomMemberCardProvider roomMemberCardProvider,
            ISubscriber<InitRoomMessage> initRoomSub,
            ISubscriber<SwitchReadyRespond> readyResSub,
            ISubscriber<ShowRoomMemberCardMessage> showCardSub,
            ISubscriber<HideMemberCardMessage> hideCardSub,
            ISubscriber<MoveMemberCardMessage> moveCardSub,
            ISubscriber<UpdateMemberNameMessage> updateNameSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            if (_cardProvider != null) return; // [과제] RectTr도 등록하느라 주입이 두 번 되는 모양인데...
            
            _cardProvider = roomMemberCardProvider;

            initRoomSub
                .Subscribe(InitSections)
                .AddTo(disposableBagBuilder);

            readyResSub
                .Subscribe(UpdateCardReadyState)
                .AddTo(disposableBagBuilder);
            
            showCardSub
                .Subscribe(AddMemberCard)
                .AddTo(disposableBagBuilder);
            
            hideCardSub
                .Subscribe(RemoveMemberCard)
                .AddTo(disposableBagBuilder);
            
            updateNameSub
                .Subscribe(UpdateCardName)
                .AddTo(disposableBagBuilder);
        }

        private void OnEnable()
        {
            var token = RefreshToken();
            ShowSections(token).Forget();
        }

        private void OnDisable()
        {
            var token = RefreshToken();
            HideSections(token).Forget();
            
            foreach (var activeCard in _activeCards.Values)
            {
                activeCard.Hide();
                _cardProvider.ReleaseCard(activeCard);
            }
            
            _activeCards.Clear();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
        
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
                        selectionSection.InitElements(msg.Mode, msg.StageIndex, msg.IsHostPlayer);
                        break;
                    case ButtonSection buttonSection:
                        buttonSection.InitElements(msg.Mode, msg.IsHostPlayer);
                        break;
                    default:
                        break;
                }
            }
        }

        private async UniTaskVoid ShowSections(CancellationToken token)
        {
            _sectionTasks.Clear();
            
            foreach (var section in sections)
            {
                _sectionTasks.Add(section.Show(token));
            }
            
            await UniTask.WhenAll(_sectionTasks);
        }

        private async UniTaskVoid HideSections(CancellationToken token)
        {
            _sectionTasks.Clear();
            
            foreach (var section in sections)
            {
                _sectionTasks.Add(section.Hide(token));
            }
            
            await UniTask.WhenAll(_sectionTasks);
        }
        
        private void AddMemberCard(ShowRoomMemberCardMessage msg)
        {
            var newCard = _cardProvider
                .GetCard(msg.SpawnPoint)
                .SetIcon(msg.MemberType)
                .SetName(msg.MemberName);

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

        private void UpdateCardName(UpdateMemberNameMessage msg)
        {
            if (string.IsNullOrEmpty(msg.Nickname)) return;
            if (!_activeCards.TryGetValue(msg.MemberId, out var targetCard)) return;
            targetCard.SetName(msg.Nickname);
        }

        private void UpdateCardPos(MoveMemberCardMessage msg)
        {
        }

        private CancellationToken RefreshToken()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }
}