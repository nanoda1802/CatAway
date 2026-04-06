using _Scripts._Shared.Enums;
using _Scripts._Shared.UI.Pop;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using _Scripts.Scene_Room.Data;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using VContainer;
using Random = UnityEngine.Random;

namespace _Scripts.Scene_Home
{
    public class RoomConnector : NetworkBehaviour
    {
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
            await UniTask.Delay(1000, cancellationToken : req.Ct); // 릴레이에서 방 생성하고 코드 받기
            
            _roomStatus.Code = $"{Random.Range(0, 256)}.{Random.Range(0, 256)}.{Random.Range(0, 256)}.{Random.Range(0, 256)}";
            
            var networkStarted = NetworkManager.StartHost();

            // 인터넷이 되거나, networkManger나 Utp가 정상인 한 이게 false가 될 일이 없어서...
            // 유효할지 고려해보기... 일단 다른 방법으로
            
            if (!networkStarted)
            {
                SendDialog("Fail To Create", _netManager.DisconnectReason, DialogButtonType.Cancel);
                
                return;
            }
        }

        private async UniTask JoinRoom(JoinRoomRequest req)
        {
            var code = string.IsNullOrEmpty(req.Code) ? "127.0.0.1" : req.Code;

            // 릴레이에서 방 찾아서 RelayData 갱신
            _utp.SetConnectionData(code,7777);
            
            await UniTask.Delay(1000, cancellationToken : req.Ct);  // 릴레이에 코드 보내서 방 찾고 UTP 설정 하기
            
            var networkStarted = NetworkManager.StartClient();

            // 인터넷이 되거나, networkManger나 Utp가 정상인 한 이게 false가 될 일이 없어서...
            // 유효할지 고려해보기... 일단 다른 방법으로
            
            if (!networkStarted)
            {
                SendDialog("Fail To Join", _netManager.DisconnectReason, DialogButtonType.Retry | DialogButtonType.Cancel);
                return;
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