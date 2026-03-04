using System;
using MessagePipe;
using TMPro;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board.Score
{
    public class ScoreBoard : MonoBehaviour, IBoard<ScoreMessage>
    {
        [SF] private Team team;
        
        [SF] private TextMeshProUGUI scoreText;
        [SF] private TextMeshProUGUI comboText;
        
        private BoardUiData _boardUiData;
        
        private int _prevCombo = -1;

        private IDisposable _subscription;
        
        [Inject]
        private void Construct(
            BoardUiData boardUiData,
            ISubscriber<ScoreMessage> sub)
        {
            _boardUiData = boardUiData;

            _subscription = sub.Subscribe(Apply, new TeamMessageFilter<ScoreMessage>(team));
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }

        public void Apply(ScoreMessage data)
        {
            UpdateScore(data.ScoreValue,data.HasPoint);
            UpdateCombo(data.ComboValue,data.HasPoint);
        }

        private void UpdateScore(int newScore, bool hasPoint)
        {
            if (hasPoint)
            {
                // 득점 트윈
            }
            else
            {
                // 감점 트윈
            }

            scoreText.text = newScore.ToString();
        }

        private void UpdateCombo(int combo, bool hasPoint)
        {
            if (_prevCombo == combo) return;
            
            combo = hasPoint ? combo : 0;
            comboText.text = string.Format(_boardUiData.ComboFormat, combo);
            
            var colorIdx = Mathf.Clamp(combo, 0, _boardUiData.ComboColorLastIndex);
            comboText.color = _boardUiData.GetComboColor(colorIdx);
            
            _prevCombo = combo;
        }
    }
}