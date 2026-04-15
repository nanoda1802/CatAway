using _Scripts.Room._Messages;
using _Scripts.Shared._Data;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI.Pop;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using VContainer;

namespace _Scripts.Room
{
    public class RoomDisconnector : NetworkBehaviour
    {
        private RoomStatus _roomStatus;
        
        private IPublisher<LoadSceneMessage> _loadScenePub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<DialogMessage> _dialogPub;
        private IPublisher<RoomToastMessage> _roomNoticePub;
        
        [Inject]
        private void Construct(
            RoomStatus roomStatus,
            IPublisher<LoadSceneMessage> loadScenePub,
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<DialogMessage> dialogPub,
            IPublisher<RoomToastMessage> roomNoticePub,
            ISubscriber<LeaveRoomMessage> leaveRoomSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _roomStatus = roomStatus;
            _loadScenePub = loadScenePub;
            _popUpPub = popUpPub;
            _dialogPub = dialogPub;
            _roomNoticePub = roomNoticePub;
            
            leaveRoomSub
                .Subscribe(LeaveRoom)
                .AddTo(disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.OnConnectionEvent += OnConnection;
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.OnConnectionEvent -= OnConnection;
            base.OnNetworkDespawn();
        }

        private async UniTaskVoid LoadHomeAfterShutdown()
        {
            NetworkManager.Shutdown();

            await UniTask.WaitUntil(() => !NetworkManager.ShutdownInProgress);
            
            var msg = new LoadSceneMessage("Home", LoadSceneMode.Single);
            _loadScenePub.Publish(msg);
        }

        private void LeaveRoom(LeaveRoomMessage req)
        {
            if (!IsServer)
            {
                LeaveRpc(NetworkManager.LocalClientId);
            }
            else
            {
                foreach (var mem in _roomStatus.ActiveMembers)
                {
                    var memId = mem.ClientId;
                    if (memId == this.OwnerClientId) continue;
                
                    NetworkManager.DisconnectClient(memId, "Host left room!");
                }   
            }
            
            LoadHomeAfterShutdown().Forget();
        }
        
        private void OnConnection(NetworkManager netMgr, ConnectionEventData eventData)
        {
            bool myEvent = netMgr.LocalClientId == eventData.ClientId;
            if (!myEvent) return;
            if (eventData.EventType != ConnectionEvent.ClientDisconnected) return;
            
            OnDisconnected(netMgr.DisconnectReason);
        }
        
        private void OnDisconnected(string reason)
        {
            NetworkManager.Shutdown();
            
            var popUpMsg = new PopUpMessage(typeof(DialogPop));
            var dialogMsg = new DialogMessage(
                "Disconnect",
                reason,
                null,
                DialogButtonType.Return);
            
            _dialogPub.Publish(dialogMsg);
            _popUpPub.Publish(popUpMsg);
        }
        
        [Rpc(SendTo.Server)]
        private void LeaveRpc(ulong targetId)
        {
            NetworkManager.ConnectedClients[targetId].PlayerObject.Despawn(true);
            NotifyDisconnectRpc(targetId);
        }

        [Rpc(SendTo.Everyone)]
        private void NotifyDisconnectRpc(ulong targetId)
        {
            var msg = new RoomToastMessage($"Player{targetId} has left Room.");
            _roomNoticePub.Publish(msg);
        }
    }
}