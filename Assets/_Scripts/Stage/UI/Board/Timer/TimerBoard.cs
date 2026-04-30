using System;
using System.Text;
using _Scripts._Helper;
using _Scripts.Stage._Data;
using _Scripts.Stage._Data.UI;
using _Scripts.Stage._Messages;
using MessagePipe;
using PrimeTween;
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
        private StageSfxListData _sfxList;
        // Dependency
        private TweenHandler _tweenHandler;
        // Component
        [SF] private Image fillBarImg;
        [SF] private TextMeshProUGUI timerTxt;
        // Caching
        private readonly StringBuilder _sb = new StringBuilder();
        private int _prevSecond = -1;
        private Sequence _curSeq;

        private bool IsTimerDirty(int newSecond) => newSecond != _prevSecond;
        private bool IsFillBarDirty(float newFillAmount) => Mathf.Abs(newFillAmount - fillBarImg.fillAmount) > _boardUiData.FillBarDirtyThreshold;
        private bool CheckFever(float remainingTime) => remainingTime < _boardUiData.FeverTime;

        [Inject]
        private void Construct(
            StageData stageData,
            BoardUiData boardUiData,
            StageSfxListData sfxList,
            TweenHandler tweenHandler,
            ISubscriber<float> timerSub,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _stageData = stageData;
            _boardUiData = boardUiData;
            _sfxList = sfxList;
            
            _tweenHandler = tweenHandler;
            
            timerSub
                .Subscribe(Apply)
                .AddTo(disposableBagBuilder);

            endSub
                .Subscribe(msg => fillBarImg.fillAmount = 0)
                .AddTo(disposableBagBuilder);
        }

        public void Apply(float data)
        {
            UpdateFillBar(data / _stageData.Duration);

            (int m, int s) = Convert(data);
            
            UpdateTimer(m,s, CheckFever(data));
        }

        private void UpdateFillBar(float ratio)
        {
            if (!IsFillBarDirty(ratio)) return;
            
            // fillBarImg.fillAmount = Mathf.Lerp(0,1,ratio);
            fillBarImg.fillAmount = ratio;
        }

        private void UpdateTimer(int minute, int second, bool isFever)
        {
            if (!IsTimerDirty(second)) return;
            
            _sb.Clear();
            _sb.AppendFormat(_boardUiData.TimerFormat, minute, second);
            timerTxt.SetText(_sb);

            if (isFever)
            {
                if (_curSeq.isAlive) _curSeq.Complete();
                _curSeq = _tweenHandler.PunchScaleWithColor(timerTxt, _boardUiData.TimerColorSettings, _boardUiData.TimerPunchSettings);
                _sfxList.Play(StageSfxType.Alarm);
            }

            _prevSecond = second;
        }

        private (int, int) Convert(float time)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            return (timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}