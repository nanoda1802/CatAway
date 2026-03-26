using System.Collections.Generic;
using System.Threading;
using _Scripts._Wrapper;
using _Scripts.Messages.Room;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Enums;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Room.UI
{
    public class StageThumbnailBoard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        // Data
        [SF] private float validSwipeDistanceRatio = 0.25f;
        [SF] private float swipeDotThreshold = 0.8f;
        // Components
        private RectTransform _boardRectTr;
        private readonly List<Thumbnail> _thumbnails = new (5); // 여유있게 지정해야
        private float _offsetX;
        // Dependency
        private StageListData _stageList;
        private IPublisher<SelectStageRequest> _selectStagePub;
        // Caching
        private bool _isDragActive;
        private Vector2 _dragStartPos;
        private CancellationTokenSource _cts;
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        [Inject]
        private void Construct(            
            StageListData stageList,
            IPublisher<SelectStageRequest> selectStagePub,
            ISubscriber<SelectStageRespond> selectStageSub)
        {
            _stageList = stageList;
            _selectStagePub = selectStagePub;

            selectStageSub
                .Subscribe(UpdateThumbnail)
                .AddTo(_disposableBagBuilder);
            
            _boardRectTr = GetComponent<RectTransform>();
            _offsetX = _boardRectTr.rect.width;

            for (int i = 0; i < this.transform.childCount; i++)
            {
                var card = this.transform.GetChild(i).GetComponent<Thumbnail>();
                _thumbnails.Add(card);
            }
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDragActive) return;
            _dragStartPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData) { } // [메모] 얘를 구현해야 Begin과 End가 작동
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragActive) return;
            if (!CheckSwipe(eventData.position, out float dot)) return;
            
            bool isLeftSwipe = dot > 0;
            
            if (isLeftSwipe) PublishRightSign();
            else PublishLeftSign();
        }

        private bool CheckSwipe(Vector2 endPos, out float dot)
        {
            dot = float.MaxValue;
            
            Vector2 diff = endPos - _dragStartPos;
            float validSwipeDist = _boardRectTr.rect.width * validSwipeDistanceRatio;
            if (diff.magnitude < validSwipeDist) return false;

            Vector2 swipeDir = diff.normalized;
            
            dot = Vector2.Dot(swipeDir, Vector2.left);
            
            return Mathf.Abs(dot) >= swipeDotThreshold;
        }
        
        public void PublishLeftSign()
        {
            var msg = new SelectStageRequest(true);
            _selectStagePub.Publish(msg);
        }

        public void PublishRightSign()
        {
            var msg = new SelectStageRequest(false);
            _selectStagePub.Publish(msg);
        }

        public void EnableSwipeBy(bool active)
        {
            _isDragActive = active;
        }

        public void InitThumbnails(StageMode mode, int idx = 0)
        {
            (_thumbnails[0].Image.sprite, _thumbnails[1].Image.sprite, _thumbnails[2].Image.sprite)
                = _stageList.GetThumbnails(mode, idx);
            
            for (int i = 0; i < _thumbnails.Count; i++)
            {
                var pos = new Vector2(_offsetX * (i - 1), 0); // (idx : posX) -> (0 : -x), (1 : 0), (2 : x)
                _thumbnails[i].RectTr.anchoredPosition = pos; 
            }
        }

        private void UpdateThumbnail(SelectStageRespond res)
        {
            var token = RefreshToken();
            
            var idxToShift = res.ToLeft ? 0 : _thumbnails.Count - 1;
            
            var shifted = _thumbnails[idxToShift];
            _thumbnails.RemoveAt(idxToShift);
            
            if (res.ToLeft) _thumbnails.Add(shifted);
            else _thumbnails.Insert(0, shifted);

            (_thumbnails[0].Image.sprite, _thumbnails[1].Image.sprite, _thumbnails[2].Image.sprite)
                = _stageList.GetThumbnails(res.CurMode, res.CurStageIndex);
            
            SlideThumbnails(shifted, token).Forget();
        }

        private async UniTaskVoid SlideThumbnails(Thumbnail shifted, CancellationToken token)
        {
            for (int i = 0; i < _thumbnails.Count; i++)
            {
                var endPos = new Vector2(_offsetX * (i - 1), 0); // (idx : posX) -> (0 : -x), (1 : 0), (2 : x)
                
                if (_thumbnails[i] == shifted)
                {
                    _thumbnails[i].RectTr.anchoredPosition = endPos;
                    continue;
                }
                
                await UniTask.Yield(token).SuppressCancellationThrow();
                _thumbnails[i].RectTr.anchoredPosition = endPos; // [임시] 트윈 예정
            }
        }

        private CancellationToken RefreshToken()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }
}