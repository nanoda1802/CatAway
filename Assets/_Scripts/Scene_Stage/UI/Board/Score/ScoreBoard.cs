using _Scripts.Scene_Stage.Data.UI;
using _Scripts.Scene_Stage.Enums;
using MessagePipe;
using TMPro;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.UI.Board.Score
{
    public class ScoreBoard : MonoBehaviour, IBoard<ScoreMessage>
    {
        [SF] private Team team;
        
        [SF] private TextMeshProUGUI scoreText;
        [SF] private TextMeshProUGUI comboText;
        
        private BoardUiData _boardUiData;
        
        private int _prevCombo = -1;
        
        [Inject]
        private void Construct(
            BoardUiData boardUiData,
            ISubscriber<ScoreMessage> scoreSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _boardUiData = boardUiData;

            scoreSub
                .Subscribe(Apply, new TeamMessageFilter<ScoreMessage>(team))
                .AddTo(disposableBagBuilder);
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