using System.Collections.Generic;
using System.Threading;
using _Scripts._Helper;
using _Scripts.Scene_Stage.Data.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.UI.Board.Order
{
    public class OrderCard : MonoBehaviour
    {
        // Component
        [SF] private CanvasGroup cardGroup;
        [SF] private RectTransform rectTr;
        [SF] private Image fillAreaImg;
        [SF] private Image fillBarImg;
        [SF] private Image[] icons;
        [SF] private Image warnCoverImg;
        // Status
        private float _duration = -1f;
        private float _orderTime = -1f;
        private int _prevTime = -1;
        // Dependency
        private TweenHandler _tweenHandler;
        private NetworkManager _netManager;
        private OrderCardData _data;
        // Property
        private bool HasValidInfo => _duration > 0f && _orderTime > 0f;
        private bool OnNetwork => _netManager is not null && _netManager.IsListening;
        private bool ShouldWarn(float ratio, int curTime) 
            => ratio < _data.StartWarnThreshold && (curTime - _prevTime) >= 1;
        
        
        [Inject]
        private void Construct(
            TweenHandler tweenHandler,
            NetworkManager netManger,
            OrderCardData data)
        {
            _tweenHandler = tweenHandler;
            _netManager = netManger;
            _data = data;
        }

        private void Update()
        {
            if (!HasValidInfo || !this.isActiveAndEnabled || !OnNetwork) return;
            var remainingRatio = CalculateRemainingTimeRatio();
            UpdateFillBar(remainingRatio);
        }

        public OrderCard ApplyOrderInfo(float duration, float orderTime)
        {
            _duration = duration;
            _orderTime = orderTime;
            return this;
        }
        
        public OrderCard ApplyIconSprites(List<Sprite> sprites)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                icons[i].sprite = sprites[i];
                icons[i].gameObject.SetActive(true);
            }
            
            return this;
        }

        public OrderCard InitStatus()
        {
            foreach (var icon in icons)
            {
                icon.gameObject.SetActive(false);
            }

            _prevTime = -1;
            _duration = _orderTime = -1f;

            rectTr.anchoredPosition = Vector2.zero;
            rectTr.localRotation = Quaternion.identity;
            rectTr.localScale = Vector3.one;
            
            return this;
        }
        
        public OrderCard SetTeamTheme(Color imgColor, Image.OriginHorizontal origin)
        {
            var bgColor = imgColor;
            bgColor.a *= 0.5f;
            
            fillAreaImg.color = bgColor;
            fillBarImg.color = imgColor;
            fillBarImg.fillOrigin = (int) origin;
            
            return this;
        }
        
        public OrderCard SetCardSize(float width, float height)
        {
            rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            return this;
        }

        private float CalculateRemainingTimeRatio()
        {
            float curServerTime = _netManager.ServerTime.TimeAsFloat;
            
            var ratio = (_duration - (curServerTime - _orderTime)) / _duration;
            
            if (ShouldWarn(ratio,(int)curServerTime)) Warn();
            _prevTime = (int)curServerTime;
            
            return ratio;
        }

        private void Warn()
        {
            _tweenHandler.ShakeWithWarning(
                rectTr,
                warnCoverImg,
                _data.ShakeScaleSettings,
                _data.ShakeRotSettings,
                _data.CoverAlphaSettings);
        }

        private void UpdateFillBar(float ratio) // [수정] Dirty 체크하기
        {
            if (ratio < 0) return;
            // fillBarImg.fillAmount = Mathf.Lerp(0, 1, ratio);
            fillBarImg.fillAmount = ratio;
        }

        public void Show(float start, float end)
        {
            var posSettings = _data.PosSettings;
            posSettings.startValue = start;
            posSettings.endValue = end;
            posSettings.settings.ease = _data.ShowEase;
            
            this.gameObject.SetActive(true);

            _tweenHandler.AnchorPosXWithAlpha(cardGroup, rectTr, _data.ShowAlphaSettings, posSettings);
        }

        public void Hide()
        {
            var curY = rectTr.anchoredPosition.y;
            var posSettings = _data.PosSettings;
            posSettings.startValue = curY;
            posSettings.endValue = curY + _data.HideOffsetY;
            posSettings.settings.ease = _data.HideEase;
            
            _tweenHandler.AnchorPosY(cardGroup, rectTr, _data.HideAlphaSettings, posSettings, () =>
            {
                InitStatus();
                this.gameObject.SetActive(false);
            });
        }
        
        public void Move(float targetX)
        {
            var curX = this.rectTr.anchoredPosition.x;
            if (Mathf.Abs(curX - targetX) <= 0.01f) return;
            
            var posSettings = _data.PosSettings;
            posSettings.startValue = curX;
            posSettings.endValue = targetX;
            posSettings.settings.ease = _data.MoveEase;
            
            _tweenHandler.AnchorPosX(rectTr, posSettings);
        }
    }
}