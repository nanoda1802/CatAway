using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts._Helper
{
    public class TweenHandler
    {
        private readonly string _defaultTextFormat = "{0}";

        public Sequence ScaleY(
            CanvasGroup canvasGroup,
            RectTransform rectTr,
            TweenSettings<float> alphaSettings,
            TweenSettings<float> scaleSettings,
            Action onComplete = null)
        {
            if (rectTr == null || !rectTr.gameObject.activeSelf || canvasGroup == null || !canvasGroup.isActiveAndEnabled)
            {
                onComplete?.Invoke();
                return default;
            }
            
            var seq = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaSettings))
                .Group(Tween.ScaleY(rectTr, scaleSettings))
                .OnComplete(onComplete);
            
            return seq;
        }
        
        public Sequence AnchorPosY(
            CanvasGroup canvasGroup,
            RectTransform rectTr,
            TweenSettings<float> alphaSettings,
            TweenSettings<float> posSettings,
            Action onComplete = null)
        {
            if (rectTr == null || !rectTr.gameObject.activeSelf || canvasGroup == null || !canvasGroup.isActiveAndEnabled)
            {
                onComplete?.Invoke();
                return default;
            }
            
            var seq = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaSettings))
                .Group(Tween.UIAnchoredPositionY(rectTr, posSettings))
                .OnComplete(onComplete);
            
            return seq;
        }
        
        public Sequence AnchorPosX(
            RectTransform rectTr,
            TweenSettings<float> posSettings,
            Action onComplete = null)
        {
            if (rectTr == null || !rectTr.gameObject.activeSelf)
            {
                onComplete?.Invoke();
                return default;
            }
            
            var seq = Sequence.Create()
                .Group(Tween.UIAnchoredPositionX(rectTr, posSettings))
                .OnComplete(onComplete);
            
            return seq;
        }
        
        public Sequence AnchorPosXWithAlpha(
            CanvasGroup canvasGroup,
            RectTransform rectTr,
            TweenSettings<float> alphaSettings,
            TweenSettings<float> posSettings,
            Action onComplete = null)
        {
            if (rectTr == null || !rectTr.gameObject.activeSelf || canvasGroup == null || !canvasGroup.isActiveAndEnabled)
            {
                onComplete?.Invoke();
                return default;
            }
            
            var seq = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaSettings))
                .Group(Tween.UIAnchoredPositionX(rectTr, posSettings))
                .OnComplete(onComplete);
            
            return seq;
        }

        public Sequence AnchorPosY(
            TextMeshProUGUI tmp,
            RectTransform rectTr,
            TweenSettings<float> alphaSettings,
            float endPosY,
            Action onComplete = null)
        {
            if (rectTr == null || !rectTr.gameObject.activeSelf || tmp == null || !tmp.isActiveAndEnabled)
            {
                onComplete?.Invoke();
                return default;
            }
            
            var seq = Sequence.Create()
                .Group(Tween.Alpha(tmp, alphaSettings))
                .Group(Tween.UIAnchoredPositionY(rectTr, endValue : endPosY, duration : alphaSettings.settings.duration))
                .OnComplete(onComplete);
            
            return seq;
        }
        
        public Tween LoopPunchScale(RectTransform rectTr, ShakeSettings settings, Action onComplete = null)
        {
            return Tween.PunchScale(rectTr, settings).OnComplete(onComplete);
        }

        public Sequence PunchScaleWithColor(TextMeshProUGUI tmp, TweenSettings<Color> colorSettings, ShakeSettings settings, Action onComplete = null)
        {
            var seq = Sequence.Create()
                .Group(Tween.Custom(target: tmp, colorSettings, (text, value) => { text.color = value; }))
                .Group(Tween.PunchScale(tmp.rectTransform, settings))
                .OnComplete(onComplete);
            
            return seq;
        }

        public Sequence Shake(
            RectTransform rectTr,
            ShakeSettings scaleSettings,
            ShakeSettings rotSettings,
            Action onComplete = null)
        {
            var seq = Sequence.Create()
                .Group(Tween.PunchScale(rectTr, scaleSettings))
                .Group(Tween.PunchLocalRotation(rectTr, rotSettings))
                .OnComplete(onComplete);
            
            return seq;
        }

        public Sequence ShakeWithWarning(
            RectTransform rectTr,
            Image img,
            ShakeSettings scaleSettings,
            ShakeSettings rotSettings,
            TweenSettings<float> alphaSettings,
            Action onShake,
            Action onComplete = null)
        {
            var seq = Sequence.Create()
                .ChainCallback(onShake)
                .Group(Tween.Alpha(img, alphaSettings))
                .Group(Tween.PunchScale(rectTr, scaleSettings))
                .Group(Tween.PunchLocalRotation(rectTr, rotSettings))
                .OnComplete(onComplete);
            
            return seq;
        }

        public Sequence Counter(
            TextMeshProUGUI tmp,
            float start,
            float end,
            TweenSettings<Color> colorSettings,
            ShakeSettings shakeSettings,
            Action onComplete = null)
        {
            var seq = Sequence.Create()
                .Group(Tween.Custom(target: tmp, colorSettings, (text, value) => { text.color = value; }))
                .Group(Tween.Custom(target: tmp, start, end, colorSettings.settings.duration,
                    (text, value) => text.SetText(_defaultTextFormat, (int)value)))
                .Chain(Tween.PunchScale(tmp.rectTransform, shakeSettings))
                .Group(Tween.Custom(target: tmp, colorSettings.endValue, colorSettings.startValue,
                    shakeSettings.duration, (text, value) => { text.color = value; }))
                .OnComplete(onComplete);
            
            return seq;
        }
    }
}