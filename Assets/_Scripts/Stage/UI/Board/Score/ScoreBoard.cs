using _Scripts._Helper;
using _Scripts.Stage._Data.UI;
using _Scripts.Stage._Enums;
using MessagePipe;
using PrimeTween;
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
        private TweenHandler _tweenHandler;
        
        private int _prevCombo = -1;
        private int _prevScore = 0;

        private Sequence _curSeq;
        
        [Inject]
        private void Construct(
            BoardUiData boardUiData,
            TweenHandler tweenHandler,
            ISubscriber<ScoreMessage> scoreSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _boardUiData = boardUiData;
            _tweenHandler = tweenHandler;

            scoreSub
                .Subscribe(Apply, new TeamMessageFilter<ScoreMessage>(team))
                .AddTo(disposableBagBuilder);

            InitBoard();
        }

        private void OnDestroy()
        {
            if (_curSeq.isAlive) _curSeq.Complete();
        }

        private void InitBoard()
        {
            scoreText.SetText("{0}", 0);
            
            comboText.SetText(_boardUiData.ComboFormat, 0);
            
            comboText.color = team switch
            {
                Team.Blue => _boardUiData.BlueTheme.TextColor,
                Team.Red => _boardUiData.RedTheme.TextColor,
                Team.None => _boardUiData.CoopTheme.TextColor,
                _ => _boardUiData.CoopTheme.TextColor
            };
        }

        public void Apply(ScoreMessage data)
        {
            UpdateScore(data.ScoreValue,data.HasPoint);
            UpdateCombo(data.ComboValue,data.HasPoint);
        }

        private void UpdateScore(int newScore, bool hasPoint)
        {
            if (newScore == _prevScore) return;

            if (_curSeq.isAlive) _curSeq.Complete();
            
            _curSeq = _tweenHandler.Counter(
                scoreText,
                _prevScore,
                newScore,
                hasPoint ? _boardUiData.AddScoreColorSettings : _boardUiData.DeductScoreColorSettings,
                _boardUiData.ScorePunchSettings,
                OnCounterComplete);
            
            _prevScore = newScore;
        }

        private void UpdateCombo(int combo, bool hasPoint)
        {
            if (_prevCombo == combo) return;
            
            combo = hasPoint ? combo : 0;
            comboText.SetText(_boardUiData.ComboFormat, combo);
            
            // var colorIdx = Mathf.Clamp(combo, 0, _boardUiData.ComboColorLastIndex);
            // comboText.color = _boardUiData.GetComboColor(colorIdx);
            
            _prevCombo = combo;
        }

        private void OnCounterComplete()
        {
            scoreText.color = _boardUiData.AddScoreColorSettings.startValue;
            scoreText.SetText("{0}", _prevScore);
        }
    }
}