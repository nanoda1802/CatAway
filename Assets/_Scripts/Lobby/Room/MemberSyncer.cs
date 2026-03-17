using _Scripts.Lobby.UI.Room;
using _Scripts.Messages.Room;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.Room
{
    public class MemberSyncer : NetworkBehaviour
    {
        [SF] private NetworkObject[] spawnPoints;
        
        private IObjectResolver _resolver;
        private RoomMember _memberPrefab;
        private RoomSyncer _roomSyncer;
        
        private IPublisher<ShowMemberCardMessage> _showCardPub;
        private IPublisher<HideMemberCardMessage> _hideCardPub;
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();

        [Inject]
        private void Construct(
            IObjectResolver resolver,
            RoomMember memberPrefab,
            RoomSyncer roomSyncer,
            IPublisher<ShowMemberCardMessage> showCardPub,
            IPublisher<HideMemberCardMessage> hideCardPub)
        {
            _resolver = resolver;
            _memberPrefab = memberPrefab;
            _roomSyncer = roomSyncer;
    
            _showCardPub = showCardPub;
            _hideCardPub = hideCardPub;
        }
        
        public override void OnNetworkSpawn()
        {
            NetworkManager.OnConnectionEvent += HandleConnectionEvent;

            var memberNetObj = _memberPrefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.AddHandler(memberNetObj, new MemberPrefabHandler(this));
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            NetworkManager.OnConnectionEvent -= HandleConnectionEvent;
            
            var memberNetObj = _memberPrefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.RemoveHandler(memberNetObj);
            
            base.OnNetworkPreDespawn();
        }

        public override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            
            base.OnDestroy();
        }

        public RoomMember CreateMemberObject(ulong clientId)
        {
            var memberObj = _resolver.Instantiate(_memberPrefab);
            memberObj.name = $"Member_{clientId}";
            return memberObj;
        }
        
        private RoomMember SpawnNewMember(ulong clientId)
        {
            var newMem = CreateMemberObject(clientId);
            var spawnIdx = _roomSyncer.InsertMember(newMem);
            var spawnPoint = spawnPoints[spawnIdx];

            var newNetObj = newMem.GetComponent<NetworkObject>();
            newNetObj.SpawnAsPlayerObject(clientId,true);
            newNetObj.TrySetParent(spawnPoint,false);
            newNetObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            return newMem;
        }
        
        private void HandleConnectionEvent(NetworkManager netMgr, ConnectionEventData eventData)
        {
            switch (eventData.EventType)
            {
                case ConnectionEvent.ClientConnected:
                    if (IsServer) OnConnectInServer(eventData.ClientId);
                    return;
                
                case ConnectionEvent.ClientDisconnected:
                    if (IsServer) OnDisconnectInServer(eventData.ClientId);
                    return;
                
                case ConnectionEvent.PeerDisconnected:
                    OnPeerDisconnect(eventData.ClientId);
                    return;
                
                default:
                    return;
            }
        }
        
        private void OnConnectInServer(ulong newClientId)
        {
            var newMem = SpawnNewMember(newClientId);
            ShowCardRpc(newClientId, newMem.IsHostMember, newMem.IsReady, newMem.CurPos, RpcTarget.Not(newClientId, RpcTargetUse.Temp));
            
            foreach (var memberId in NetworkManager.ConnectedClients.Keys)
            {
                var mem = _roomSyncer.FindMember(memberId);
                ShowCardRpc(memberId, mem.IsHostMember, mem.IsReady, mem.CurPos, RpcTarget.Single(newClientId, RpcTargetUse.Temp));
            }
        }

        private void OnDisconnectInServer(ulong targetId) // PlayerPrefab이라 자동으로 디스폰되는 바람에 직접 디스폰은 안하지만...
        {
            if (_roomSyncer is not { IsSpawned : true }) return;
            
            _roomSyncer.RemoveMember(targetId);
        }
        
        private void OnPeerDisconnect(ulong targetId)
        {
            var msg = new HideMemberCardMessage(targetId);
            _hideCardPub.Publish(msg);
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void ShowCardRpc(ulong memberId, bool isHostMember, bool isReady, Vector3 pos, RpcParams rpcParams)
        {
            var iconType = MemberIconType.NonReady;
            
            if (isHostMember) 
                iconType = MemberIconType.Host;
            else if (isReady) 
                iconType = MemberIconType.Ready;
            
            var msg = new ShowMemberCardMessage(memberId, iconType, pos);
            _showCardPub.Publish(msg);
        }
    }
}