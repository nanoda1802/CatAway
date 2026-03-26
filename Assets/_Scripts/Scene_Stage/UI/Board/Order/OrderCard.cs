using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        [SF] private RectTransform rectTr;
        [SF] private Image fillAreaImg;
        [SF] private Image fillBarImg;
        [SF] private Image[] icons;
        // Status
        private float _duration = -1f;
        private float _orderTime = -1f;
        // Dependency
        private NetworkManager _netManager;
        // Property
        private bool HasValidInfo => _duration > 0f && _orderTime > 0f;

        [Inject]
        private void Construct(NetworkManager netManger)
        {
            _netManager = netManger;
        }

        private void Update()
        {
            if (!HasValidInfo || !this.isActiveAndEnabled) return;
            UpdateFillBar();
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

            _duration = _orderTime = -1f;
            
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

        private void UpdateFillBar() // [수정] Dirty 체크하기
        {
            if (_netManager is null || !_netManager.IsListening) return;
            var curServerTime = _netManager.ServerTime.TimeAsFloat;
            var ratio = (_duration - (curServerTime - _orderTime)) / _duration;
            
            if (ratio < 0) return;
            fillBarImg.fillAmount = Mathf.Lerp(0, 1, ratio);
        }

        public async UniTask Show(Vector2 pos, CancellationToken token) 
        {
            this.rectTr.anchoredPosition = pos;
            
            this.gameObject.SetActive(true);
            
            await UniTask.Yield(cancellationToken:token);  // [수정] 등장 트윈 추가
        }

        public async UniTask Hide(CancellationToken token)
        {
            this.gameObject.SetActive(false);
            
            this.transform.SetAsLastSibling();
            
            await UniTask.Yield(cancellationToken:token); // [수정] 퇴장 트윈 대기
        }

        public async UniTask Move(Vector2 targetPos, CancellationToken token)
        {
            var curPos = this.rectTr.anchoredPosition;
            if ((curPos - targetPos).sqrMagnitude <= 0.1f) return;
            
            this.rectTr.anchoredPosition = targetPos;
            
            await UniTask.Yield(cancellationToken:token); // [수정] 이동 트윈 대기
        }
    }
}