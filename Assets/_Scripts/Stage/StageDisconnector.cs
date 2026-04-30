using _Scripts.Room._Messages;
using _Scripts.Shared._Data;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.Sound;
using _Scripts.Shared.UI.Pop;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using VContainer;

namespace _Scripts.Stage
{
    public class StageDisconnector : NetworkBehaviour
    {
        private RoomStatus _roomStatus;
        private SoundManager _soundManager;
        
        private IPublisher<LoadSceneMessage> _loadScenePub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<DialogMessage> _dialogPub;
        
        [Inject]
        private void Construct(
            RoomStatus roomStatus,
            SoundManager soundManager,
            IPublisher<LoadSceneMessage> loadScenePub,
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<DialogMessage> dialogPub,
            ISubscriber<LeaveRoomMessage> leaveRoomSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _roomStatus = roomStatus;
            _soundManager = soundManager;
            
            _loadScenePub = loadScenePub;
            _popUpPub = popUpPub;
            _dialogPub = dialogPub;
            
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

            StopSounds();
            LoadHomeAfterShutdown().Forget();
        }

        private void StopSounds()
        {
            _soundManager.StopBgm().Forget();
        }

        private void OnConnection(NetworkManager netMgr, ConnectionEventData eventData)
        {
            if (eventData.EventType != ConnectionEvent.ClientDisconnected) return;

            if (IsServer)
            {
                _roomStatus.RemoveMember(eventData.ClientId, out int idx);
            }
            
            if (netMgr.LocalClientId == eventData.ClientId) 
                OnDisconnected(netMgr.DisconnectReason);
        }
        
        private void OnDisconnected(string reason)
        {
            StopSounds();

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
        }
    }
}