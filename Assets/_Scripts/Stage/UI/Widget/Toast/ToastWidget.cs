using _Scripts._Helper;
using _Scripts.Stage._Data.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.Stage.UI.Widget.Toast
{
    public class ToastWidget : WidgetBase
    {
        private TextMeshProUGUI _toastTxt;

        private TweenHandler _tweenHandler;
        private WidgetData _data;
        private StageHub _stageHub;

        private Sequence _curSeq;
        
        [Inject]
        private void Construct(
            TweenHandler tweenHandler,
            WidgetData data,
            StageHub stageHub)
        {
            _toastTxt = GetComponent<TextMeshProUGUI>();
            
            _tweenHandler = tweenHandler;
            _data = data;
            _stageHub = stageHub;
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            if (_curSeq.isAlive) _curSeq.Complete();
            
            base.Hide();
        }

        public override void UpdatePosition(Vector3 worldPos)
        {
            base.UpdatePosition(worldPos + _data.ToastOffset);
        }

        public void SetText(string text, bool isPositive)
        {
            _toastTxt.SetText(text);
            _toastTxt.color = isPositive ? _data.PositiveColor : _data.NegativeColor;
            
            _curSeq = _tweenHandler.AnchorPosY(
                _toastTxt,
                WidgetRectTr,
                _data.ToastAlphaSettings,
                WidgetRectTr.anchoredPosition.y + _data.ToastMoveDist,
                ReleaseThis);
        }

        private void ReleaseThis()
        {
            var provider = _stageHub.FetchProvider<ToastProvider>();
            provider.ReleaseWidget(this);
        }
    }
}