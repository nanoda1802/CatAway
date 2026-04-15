using _Scripts._Helper;
using _Scripts.Stage._Data.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Widget.TableAlert
{
    public class TableAlertWidget : WidgetBase
    {
        [SF] private Image alertImg;
        
        private TweenHandler _tweenHandler;
        private WidgetData _data;

        private Tween _curTween;
        
        [Inject]
        private void Construct(
            TweenHandler tweenHandler,
            WidgetData data)
        {
            _tweenHandler = tweenHandler;
            _data = data;
        }

        public override void Show()
        {
            base.Show();
            _curTween = _tweenHandler.LoopPunchScale(WidgetRectTr, _data.AlertTweenSettings, OnTweenCompleted);
        }

        public override void Hide()
        {
            if (_curTween.isAlive) _curTween.Complete();
            base.Hide();
        }

        public override void UpdatePosition(Vector3 worldPos)
        {
            base.UpdatePosition(worldPos + _data.TableAlertOffset);
        }

        private void OnTweenCompleted()
        {
            WidgetRectTr.localScale = Vector3.one;
        }
    }
}