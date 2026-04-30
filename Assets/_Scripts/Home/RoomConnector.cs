using System;
using System.Threading;
using _Scripts.Room._Messages;
using _Scripts.Shared._Data;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI.Pop;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using VContainer;
using Random = UnityEngine.Random;

namespace _Scripts.Home
{
    public class RoomConnector : NetworkBehaviour
    {
        private const int ConnectTimeoutMs = 3500;
        
        private NetworkManager _netManager;
        private UnityTransport _utp;
        
        private RoomStatus _roomStatus;

        private IPublisher<LoadSceneMessage> _loadScenePub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<PopDownMessage> _popDownPub;
        private IPublisher<DialogMessage> _dialogPub;
        
        [Inject]
        private void Construct(
            RoomStatus roomStatus,
            NetworkManager netManager,
            UnityTransport utp,
            IPublisher<LoadSceneMessage> loadScenePub,
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<PopDownMessage> popDownPub,
            IPublisher<DialogMessage> dialogPub,
            ISubscriber<CreateRoomRequest> createSub,
            ISubscriber<JoinRoomRequest> joinSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _roomStatus = roomStatus;
            _netManager = netManager;
            _utp = utp;
            _loadScenePub = loadScenePub;
            _popUpPub = popUpPub;
            _popDownPub = popDownPub;
            _dialogPub = dialogPub;
            
            createSub
                .Subscribe(req => CreateRoom(req).Forget())
                .AddTo(disposableBagBuilder);
            
            joinSub
                .Subscribe(req => JoinRoom(req).Forget())
                .AddTo(disposableBagBuilder);
            
            _netManager.OnConnectionEvent += OnConnection;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            _loadScenePub.Publish(new LoadSceneMessage("Room", LoadSceneMode.Single));
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _netManager.OnConnectionEvent -= OnConnection;
            base.OnNetworkDespawn();
        }

        private void OnConnection(NetworkManager netMgr, ConnectionEventData eventData)
        {
            bool myEvent = netMgr.LocalClientId == eventData.ClientId;
            if (!myEvent) return;
            
            switch (eventData.EventType)
            {
                case ConnectionEvent.ClientConnected:
                    if (!netMgr.IsApproved)
                    {
                        SendDialog("Fail To Join", netMgr.DisconnectReason, DialogButtonType.Retry | DialogButtonType.Cancel);
                        return;
                    }
                    _popDownPub.Publish(new PopDownMessage());
                    return;
                
                case ConnectionEvent.ClientDisconnected:
                    if (!netMgr.IsApproved)
                    {
                        SendDialog("Fail To Join", netMgr.DisconnectReason, DialogButtonType.Retry | DialogButtonType.Cancel);
                        return;
                    }
                    return;
                
                default:
                    return;
            }
        }
        
        private async UniTask CreateRoom(CreateRoomRequest req)
        {
            await UniTask.Delay(500, cancellationToken: req.Ct); // 릴레이에서 방 생성하고 코드 받기
            
            _roomStatus.Code = $"{Random.Range(0, 256)}.{Random.Range(0, 256)}.{Random.Range(0, 256)}.{Random.Range(0, 256)}";
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(req.Ct);
            linkedCts.CancelAfterSlim(ConnectTimeoutMs);

            var networkStarted = _netManager.StartHost();
            
            if (!networkStarted)
            {
                SendDialog("Fail To Create", _netManager.DisconnectReason, DialogButtonType.Cancel);
                return;
            }

            bool canceled = await UniTask.WaitUntil(() => _netManager.IsServer || _netManager.IsConnectedClient, cancellationToken: linkedCts.Token).SuppressCancellationThrow();
            
            if (canceled && this != null && !IsSpawned)
            {
                _netManager.Shutdown();
                if (!req.Ct.IsCancellationRequested)
                {
                    SendDialog("Fail To Create", "The operation timed out.", DialogButtonType.Cancel);
                }
            }
        }

        private async UniTask JoinRoom(JoinRoomRequest req)
        {
            // 요청받은 코드 정규식 검사
            // 아니면 _utp.SetConnectionData() 여기서 문제 발생
            // 트랜스포트가 비정상 작동해서 포트를 점거해버림
            
            var code = string.IsNullOrEmpty(req.Code) ? "127.0.0.1" : req.Code;
            
            _utp.SetConnectionData(code, 7777); // 릴레이에서 방 찾아서 RelayData 갱신
            
            await UniTask.Delay(500, cancellationToken: req.Ct);  // 릴레이에 코드 보내서 방 찾고 UTP 설정 하기
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(req.Ct);
            linkedCts.CancelAfterSlim(ConnectTimeoutMs);

            var networkStarted = _netManager.StartClient();

            if (!networkStarted)
            {
                SendDialog("Fail To Join", _netManager.DisconnectReason, DialogButtonType.Retry | DialogButtonType.Cancel);
                return;
            }

            bool canceled = await UniTask.WaitUntil(() => _netManager.IsConnectedClient, cancellationToken: linkedCts.Token).SuppressCancellationThrow();
            
            if (canceled && this != null && !IsSpawned)
            {
                _netManager.Shutdown();
                if (!req.Ct.IsCancellationRequested)
                {
                    SendDialog("Fail To Join", "The operation timed out.", DialogButtonType.Retry | DialogButtonType.Cancel);
                }
            }
        }

        private void SendDialog(string header, string reason, DialogButtonType requiredButton)
        {
            var popUpMsg = new PopUpMessage(typeof(DialogPop));
            var dialogMsg = new DialogMessage(
                header,
                reason,
                string.Empty,
                requiredButton
            );
                
            _popUpPub.Publish(popUpMsg);
            _dialogPub.Publish(dialogMsg);
        }
    }
}