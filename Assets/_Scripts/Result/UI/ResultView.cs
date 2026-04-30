using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using _Scripts.Result._Data;
using _Scripts.Result._Messages;
using _Scripts.Shared._Messages;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Result.UI
{
    public class ResultView : MonoBehaviour
    {
        [SF] private TextMeshProUGUI timerTxt;
        [SF] private Image coverImg;

        // Dependency
        private ResultMemberCardProvider _cardProvider;
        private ResultViewData _viewData;
        // Caching
        private readonly Dictionary<ulong, ResultMemberCard> _activeCards = new();
        private CancellationTokenSource _cts;
        private readonly StringBuilder _sb = new StringBuilder();
        private int _prevSecond = -1;

        private bool IsTimerDirty(int newSecond) => newSecond != _prevSecond;
        
        [Inject]
        private void Construct(
            ResultMemberCardProvider cardProvider,
            ResultViewData viewData,
            ISubscriber<ShowResultMemberCardMessage> showSub,
            ISubscriber<HideMemberCardMessage> hideSub,
            ISubscriber<MoveMemberCardMessage> moveSub,
            ISubscriber<UpdateMemberNameMessage> updateNameSub,
            ISubscriber<float> timerSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _cardProvider = cardProvider;
            _viewData = viewData;
            
            showSub
                .Subscribe(AddMemberCard)
                .AddTo(disposableBagBuilder);
            
            hideSub
                .Subscribe(RemoveMemberCard)
                .AddTo(disposableBagBuilder);
            
            moveSub
                .Subscribe(UpdateCardPos)
                .AddTo(disposableBagBuilder);
            
            updateNameSub
                .Subscribe(UpdateCardName)
                .AddTo(disposableBagBuilder);
            
            timerSub
                .Subscribe(UpdateTimer)
                .AddTo(disposableBagBuilder);
            
            coverImg.transform.SetAsLastSibling();
        }
        
        private int Convert(float value)
        {
            var timeSpan = TimeSpan.FromSeconds(value);
            return (int) timeSpan.TotalSeconds;
        }

        private void UpdateTimer(float time)
        {
            int second = Convert(time);

            if (!IsTimerDirty(second)) return;
            
            if (second <= _viewData.SpareTimeAfterSkip) timerTxt.color = _viewData.SpareTimeColor;
            
            _sb.Clear();
            _sb.AppendFormat(_viewData.TimerFormat, second);
            timerTxt.SetText(_sb);

            _prevSecond = second;
        }
        
        private void AddMemberCard(ShowResultMemberCardMessage msg)
        {
            var nickname = 
                string.IsNullOrEmpty(msg.Name) 
                ? $"Player{msg.MemberId}"
                : msg.Name;
            
            var newCard = _cardProvider.GetCard(msg.SpawnPoint)
                .SetTeamTheme(msg.Team)
                .SetName(nickname)
                .SetAceIcon(msg.IsAce);

            _activeCards[msg.MemberId] = newCard;
            
            newCard.Show();
        }

        private void RemoveMemberCard(HideMemberCardMessage msg)
        {
            if (!_activeCards.Remove(msg.MemberId, out var targetCard)) return;
            
            targetCard.Hide();
            
            _cardProvider.ReleaseCard(targetCard);
        }

        private void UpdateCardPos(MoveMemberCardMessage msg)
        {
            if (!_activeCards.TryGetValue(msg.MemberId, out var card)) return;
            
            card.UpdatePosition(msg.NewPos);
        }
        
        private void UpdateCardName(UpdateMemberNameMessage msg)
        {
            if (string.IsNullOrEmpty(msg.Nickname)) return;
            if (!_activeCards.TryGetValue(msg.MemberId, out var targetCard)) return;
            targetCard.SetName(msg.Nickname);
        }
    }
}