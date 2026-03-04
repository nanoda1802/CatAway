using System;
using System.Text;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board.Timer
{
    public class TimerBoard : MonoBehaviour, IBoard<float>
    {
        // Data
        private StageData _stageData;
        private BoardUiData _boardUiData;
        // Component
        [SF] private Image fillBarImg;
        [SF] private TextMeshProUGUI timerTxt;
        // Caching
        private readonly StringBuilder _sb = new StringBuilder();
        private int _prevSecond = -1;
        private IDisposable _subscription;

        private bool IsTimerDirty(int newSecond) => newSecond != _prevSecond;
        private bool IsFillBarDirty(float newFillAmount) => Mathf.Abs(newFillAmount - fillBarImg.fillAmount) > _boardUiData.FillBarDirtyThreshold;

        [Inject]
        private void Construct(
            StageData stageData,
            BoardUiData boardUiData,
            ISubscriber<float> sub)
        {
            _stageData = stageData;
            _boardUiData = boardUiData;
            
            _subscription = sub.Subscribe(Apply);
        }
        
        private void OnDestroy()
        {
            _subscription?.Dispose();
        }
        
        public void Apply(float data)
        {
            UpdateFillBar(data / _stageData.Duration);

            (int m, int s) = Convert(data);
            
            UpdateTimer(m,s);
        }

        private void UpdateFillBar(float ratio)
        {
            if (!IsFillBarDirty(ratio)) return;
            
            fillBarImg.fillAmount = Mathf.Lerp(0,1,ratio);
        }

        private void UpdateTimer(int minute, int second)
        {
            if (!IsTimerDirty(second)) return;
            
            _sb.Clear();
            _sb.AppendFormat(_boardUiData.TimerFormat, minute, second);
            timerTxt.text = _sb.ToString();

            _prevSecond = second;
        }

        private (int, int) Convert(float time)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            return (timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}