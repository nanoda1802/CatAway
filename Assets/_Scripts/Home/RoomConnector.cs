using System;
using System.Threading;
using _Scripts.Room._Messages;
using _Scripts.Shared;
using _Scripts.Shared._Data;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI.Pop;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Random = UnityEngine.Random;

namespace _Scripts.Home
{
    public class RoomConnector : NetworkBehaviour
    {
        private const int ConnectTimeoutMs = 5000;
        
        private NetworkManager _netManager;
        private UnityTransport _utp;
        private AuthManager _authManager;
        
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
            AuthManager authManager,
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
            _authManager = authManager;
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
            RelayServerData relayData;
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(req.Ct);
            linkedCts.CancelAfterSlim(ConnectTimeoutMs);
            
            try
            {
                relayData = await _authManager.AllocateRelayServerAndGetJoinCode(4, linkedCts.Token);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Relay Connect Failed : {e.Message}");
                SendDialog("Fail To Create", e.Message, DialogButtonType.Cancel);
                return;
            }
            
            Debug.Log($"Relay Connect Success : {_authManager.RoomCode}");
            _roomStatus.Code = _authManager.RoomCode;
            
            _utp.SetRelayServerData(relayData);
            
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
            RelayServerData relayData;
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(req.Ct);
            linkedCts.CancelAfterSlim(ConnectTimeoutMs);

            try
            {
                relayData = await _authManager.JoinRelayServerFromJoinCode(req.Code, linkedCts.Token);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Relay Connect Failed : {e.Message}");
                SendDialog("Fail To Join", e.Message, DialogButtonType.Retry | DialogButtonType.Cancel);
                return;
            }
            
            _utp.SetRelayServerData(relayData);
            
            // var code = string.IsNullOrEmpty(req.Code) ? "127.0.0.1" : req.Code;
            
            // _utp.SetConnectionData(code, 7777); // 릴레이에서 방 찾아서 RelayData 갱신
            
            // await UniTask.Delay(500, cancellationToken: req.Ct);  // 릴레이에 코드 보내서 방 찾고 UTP 설정 하기
            
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