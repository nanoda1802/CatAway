using System;
using System.Collections.Generic;
using System.Threading;
using _Scripts.Lobby.UI.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;
using VContainer.Unity;

namespace _Scripts.Lobby.UI
{
    public class ViewChanger : IInitializable, IDisposable
    {
        private readonly TransitionWindow _transitionWindow;
        private readonly QuickMenuBar _quickMenuBar;
        private readonly Dictionary<Type, IView> _viewDic = new();

        private IView _curView;
        
        private readonly IPublisher<PopDownMessage> _popDownPub;
        
        private CancellationTokenSource _cts;
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        public ViewChanger(
            // TransitionWindow transitionWindow,
            QuickMenuBar quickMenuBar,
            IReadOnlyList<IView> viewList,
            IPublisher<PopDownMessage> popDownPub,
            ISubscriber<ChangeViewRequest> viewChangeSub)
        {
            // _transitionWindow = transitionWindow;
            _quickMenuBar = quickMenuBar;

            foreach (var view in viewList)
                _viewDic.TryAdd(view.GetType(), view);
            
            _popDownPub = popDownPub;

            viewChangeSub
                .Subscribe(req => ChangeTo(req).Forget())
                .AddTo(_disposableBagBuilder);
        }

        public void Initialize()
        { 
            this.ChangeTo(new ChangeViewRequest(typeof(TitleView))).Forget();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _disposableBagBuilder?.Build().Dispose();
        }

        // view 전환 시작 메서드
        public async UniTaskVoid ChangeTo(ChangeViewRequest req)
        {
            var token = RefreshToken();
            
            if (_curView != null) await _curView.Deactivate(token);
            
            _popDownPub.Publish(new PopDownMessage()); // [임시]
            
            var newView = _viewDic[req.ViewType];
            
            _quickMenuBar.RefreshBtnGroup(newView.RequiredQuickMenu);
            _curView = newView;
            
            await newView.Activate(token);
            
            // 1. 트랜지션 화면 활성화 _transitionWindow.Activate();
            // 2. 기존 팝업 닫기 _popDownPub.Publish(new PopDownMessage());
            // 3. TitleCam < RoomCam 되도록 vCam priority 조정
        }

        public async UniTask EndTransition()
        {
            await UniTask.Yield();
            
            // 7. PunchHole 이미지 originPos 조정 (Title이면 화면 중앙, Room이면 플레이어의 RoomMember 오브젝트 중앙)
            // 8. PunchHole 이미지 `scale` 트윈
            // 9. 트랜지션 화면 활성화 _transitionWindow.Deactivate();
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