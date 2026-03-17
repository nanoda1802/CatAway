using System;
using System.Linq;
using _Scripts.Lobby.UI;
using _Scripts.Lobby.UI.Pop;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Collections;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Scripts.Lobby.Room
{
    public class RoomConnector : NetworkBehaviour
    {
        private NetworkManager _netManager; // 비 네트워크 상황에서 활용
        private UnityTransport _utp;
        private RoomSyncer _roomSyncer;

        private readonly NetworkVariable<FixedString32Bytes> _sharedCode = new();
        
        private IPublisher<InitRoomMessage> _initRoomPub;
        private IPublisher<ChangeViewRequest> _changeViewPub;
        private IPublisher<DialogMessage> _dialogPub;
        private IPublisher<PopUpMessage> _popUpPub;

        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        private string Code
        {
            get => _sharedCode.Value.Value;
            set => _sharedCode.Value = value;
        }

        [Inject]
        private void Construct(
            NetworkManager networkManager,
            UnityTransport utp,
            RoomSyncer roomSyncer,
            IPublisher<InitRoomMessage> initRoomPub,
            IPublisher<ChangeViewRequest> changeViewPub,
            IPublisher<DialogMessage> dialogPub,
            IPublisher<PopUpMessage> popUpPub,
            ISubscriber<CreateRoomRequest> createSub,
            ISubscriber<JoinRoomRequest> joinSub,
            ISubscriber<LeaveRoomRequest> leaveSub)
        {
            _netManager = networkManager;
            _utp = utp;
            _roomSyncer = roomSyncer;
            _initRoomPub = initRoomPub;
            _changeViewPub = changeViewPub;
            _dialogPub = dialogPub;
            _popUpPub = popUpPub;
            
            createSub
                .Subscribe(req => CreateRoom(req).Forget())
                .AddTo(_disposableBagBuilder);
            
            joinSub
                .Subscribe(req => JoinRoom(req).Forget())
                .AddTo(_disposableBagBuilder);
            
            leaveSub
                .Subscribe(req => LeaveRoom(req).Forget())
                .AddTo(_disposableBagBuilder);
        }

        private void Awake()
        {
            var objects = FindObjectsByType<RoomConnector>(FindObjectsSortMode.None);
            
            if (objects.Length > 1)
            {
                Destroy(this.gameObject);
                return; 
            }
            
            DontDestroyOnLoad(this.gameObject);
        }

        private void OnEnable()
        {
            Debug.Log("RoomConnector Enabled : Relay Connect Request");
            
            _netManager.NetworkConfig.ConnectionApproval = true; // 네트워크가 이미 시작됐으면 config을 수정할 수 없어서 미리!
            _netManager.OnConnectionEvent += HandleConnectionEvent; // 이건 미리 해야하네!
        }

        private void OnDisable()
        {
            _netManager.OnConnectionEvent -= HandleConnectionEvent;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.ConnectionApprovalCallback = ApprovalCheck;
            }

            var msg = new InitRoomMessage(Code, _roomSyncer.CurMode, _roomSyncer.CurStageIndex, IsHost);
            _initRoomPub.Publish(msg);
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            if (IsServer)
            {
                NetworkManager.ConnectionApprovalCallback = null;
            }
            
            base.OnNetworkPreDespawn();
        }
        
        public override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            
            base.OnDestroy();
        }

        private async UniTask CreateRoom(CreateRoomRequest req)
        {
            await UniTask.Delay(2000, cancellationToken : req.Ct); // 릴레이에서 방 생성하고 코드 받기
            
            Code = $"{Random.Range(0, 256)}.{Random.Range(0, 256)}.{Random.Range(0, 256)}.{Random.Range(0, 256)}";
            
            _roomSyncer.InitStageSelection();
            
            var networkStarted = NetworkManager.StartHost();

            // 인터넷이 되거나, networkManger나 Utp가 정상인 한 이게 false가 될 일이 없어서...
            // 유효할지 고려해보기... 일단 다른 방법으로
            
            if (!networkStarted)
            {
                // DoSomething
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
                // DoSomething
                return;
            }
        }

        private async UniTask LeaveRoom(LeaveRoomRequest req)
        {
            await UniTask.Delay(500);

            if (IsHost)
            {
                NotifyHostLeftRpc(); // 이거 먼저보내야하네 생각해보니
                
                foreach (var mem in _roomSyncer.ActiveMembers)
                {
                    var memId = mem.OwnerClientId;
                    if (memId == this.OwnerClientId) continue;
                    
                    NetworkManager.DisconnectClient(memId, "Host left room!");
                }
                
                NetworkManager.Shutdown();
            }
            else
            {
                LeaveRpc(req.ClientId);
            }

            var msg = new ChangeViewRequest(typeof(TitleView));
            _changeViewPub.Publish(msg);
        }

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest req,
            NetworkManager.ConnectionApprovalResponse res)
        {
            res.Approved = false;
            res.Pending = false; // 추가 검증 없으니 대기 방지
            
            if (_roomSyncer is not { IsSpawned : true })
            {
                res.Reason = "Requested room is not yet spawned.";
                return;
            }
            
            if (NetworkManager.ConnectedClients.Count >= 4) // [임시] 테스트용! 잊지말고 수정해주기
            {
                res.Reason = "Max Clients connected.";
                return;
            }
            
            if (_roomSyncer.IsFull)
            {
                res.Reason = "Requested room is already full.";
                return;
            }

            if (NetworkManager.ConnectedClientsIds.Contains(req.ClientNetworkId))
            {
                res.Reason = "Duplicated client ID..?";
                return;
            }

            if (false) // [추가] 스테이지 시작된 룸인 경우
            {
                res.Reason = "Requested room is already started";
                return;
            }

            res.Approved = true; // 접속 승인
            res.CreatePlayerObject = false; // 자동 생성 방지
        }

        private void HandleConnectionEvent(NetworkManager netMgr, ConnectionEventData eventData)
        {
            switch (eventData.EventType)
            {
                case ConnectionEvent.ClientConnected:
                    OnConnected(netMgr.IsApproved, netMgr.LocalClientId, eventData.ClientId);
                    return;
                
                case ConnectionEvent.ClientDisconnected:
                    if (!netMgr.IsServer) OnDisconnectedInClient(netMgr.IsApproved, netMgr.DisconnectReason);
                    return;
                
                default:
                    return;
            }
        }

        private void OnConnected(bool approved, ulong localId, ulong eventId)
        {
            var isMyEvent = localId == eventId;
            
            if (!isMyEvent)
            {
                Debug.LogWarning($"[RoomConnector.OnConnected] Not My Connection Event!"); 
                return;
            }
            
            if (!approved)
            {
                Debug.LogWarning($"[RoomConnector.OnConnected] This Connection is not Approved!"); 
                return;
            }

            var msg = new ChangeViewRequest(typeof(RoomView));
            _changeViewPub.Publish(msg);
        }

        private void OnDisconnectedInClient(bool approved, string reason)
        {
            // if (isApproved) return;
            // [수정] 왜 거절 당했는지 Dialog에 표시...!
            
            Debug.LogWarning($"[RoomConnector.OnDisconnected] Approval? {approved} / reason? {reason}");
        }

        [Rpc(SendTo.Server)]
        private void LeaveRpc(ulong targetId)
        {
            NetworkManager.ConnectedClients[targetId].PlayerObject.Despawn(true);
            NetworkManager.DisconnectClient(targetId, "The Client left room by self.");
        }

        [Rpc(SendTo.NotServer)]
        private void NotifyHostLeftRpc()
        {
            var popUpMsg = new PopUpMessage(typeof(DialogPop));
            var dialogMsg = new DialogMessage(
                "Notice",
                "Host left this room!",
                null,
                DialogButtonType.Return);
            
            _dialogPub.Publish(dialogMsg);
            _popUpPub.Publish(popUpMsg);
        }
    }
}