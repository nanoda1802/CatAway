using System.Collections.Generic;
using System.Linq;
using _Scripts._Messages.Room;
using _Scripts._Messages.Shared;
using _Scripts.Messages.Room;
using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Room.Enums;
using _Scripts.Scene_Stage.Enums;
using MessagePipe;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Room
{
    public class RoomMemberSyncer : NetworkBehaviour
    {
        [SF] private float[] spawnPosX; // 3,1,-1,-3
        [SF] private float[] spawnRotY; // -30,-15, 15, 30
        [SF] private float spawnPosZ = -5.5f;
        
        private IObjectResolver _resolver;
        private RoomMember _memberPrefab;
        private RoomStatus _roomStatus;
        
        private IPublisher<SwitchStartMessage> _startSwitchPub;
        
        private readonly Dictionary<ulong, RoomMember> _members = new();

        private bool IsAllMemReady
        {
            get
            {
                foreach (RoomMember mem in _members.Values)
                {
                    if (mem is null) continue;
                    if (!mem.IsReady) return false;
                }
                
                return true;
            }
        }

        public bool CanStartStage
        {
            get
            {
                return true;
                if (!IsServer) return false;
                if (!_roomStatus.EachTeamHasMember) return false;

                return IsAllMemReady;
            }
        }
        
        
        [Inject]
        private void Construct(
            IObjectResolver resolver,
            RoomMember memberPrefab,
            RoomStatus roomStatus,
            IPublisher<SwitchStartMessage> switchStartPub,
            ISubscriber<SwitchModeRequest> switchModeSub,
            ISubscriber<SwitchReadyRespond>  switchReadySub,
            DisposableBagBuilder disposableBagBuilder
            )
        {
            _resolver = resolver;
            _memberPrefab = memberPrefab;
            _roomStatus = roomStatus;
            
            _startSwitchPub = switchStartPub;
            
            switchModeSub
                .Subscribe(InitReadyStates)
                .AddTo(disposableBagBuilder);
            
            switchReadySub
                .Subscribe(CheckStartState)
                .AddTo(disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.SceneManager.OnLoadEventCompleted += OnRoomLoadComplete;
            }
            
            RegisterPrefabHandler();
            NetworkManager.OnConnectionEvent += OnConnection;

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            UnregisterPrefabHandler();
            NetworkManager.OnConnectionEvent -= OnConnection;
            base.OnNetworkDespawn();
        }

        private void OnRoomLoadComplete( // 씬에 배치된 스폰포인트가 네트워크매니저를 인식하기 전에 이 녀석의 postSpawn이 발동해버려
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (!IsServer) return;
            if (sceneName != "Room") return;
            
            Debug.Log($"- - - - - - <color=red>[RoomMemSyncer]</color> LoadCompleted - - - - - -");
            
            var activeMem = _roomStatus.ActiveMembers.ToList();
            
            if (activeMem.Count <= 0)
            {
                activeMem.Add(new MemberInfo(NetworkManager.LocalClientId)); // [임시]
            }
            
            foreach (var mem in activeMem)
            {
                AddMember(mem.ClientId);
            }   
            
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnRoomLoadComplete;
            Debug.Log($"- - - - - - <color=red>[RoomMemSyncer]</color> LoadCompleted - - - - - -");
        }
        
        public RoomMember CreateMemberObject(ulong clientId, Vector3 pos, Quaternion rot)
        {
            var memberObj = _resolver.Instantiate(_memberPrefab, pos, rot);
            memberObj.name = $"Member_{clientId}";
            return memberObj;
        }
        
        private RoomMember SpawnNewMember(ulong clientId, int spawnIdx)
        {
            (Vector3 spawnPos, Quaternion spawnRot) = CalculatePosAndRot(spawnIdx);
            var newMem = CreateMemberObject(clientId, spawnPos, spawnRot).AssignTo(this);
            
            var newNetObj = newMem.GetComponent<NetworkObject>();
            newNetObj.SpawnAsPlayerObject(clientId,true);
            
            return newMem;
        }
        
        private (Vector3, Quaternion) CalculatePosAndRot(int idx)
        {
            Vector3 pos = new Vector3(spawnPosX[idx], 0, spawnPosZ);
            float rotY = spawnRotY[idx];
            Quaternion rot = Quaternion.Euler(0, rotY, 0);
            return (pos, rot);
        }
        
        private void OnConnection(NetworkManager netMgr, ConnectionEventData eventData)
        {
            switch (eventData.EventType)
            {
                case ConnectionEvent.ClientConnected:
                    if (IsServer) AddMember(eventData.ClientId);
                    return;
                
                case ConnectionEvent.ClientDisconnected:
                    if (IsServer) RemoveTargetMember(eventData.ClientId);
                    return;
                
                default:
                    return;
            }
        }

        private void RegisterPrefabHandler()
        {
            var memberNetObj = _memberPrefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.AddHandler(memberNetObj, new RoomMemberPrefabHandler(this));
        }

        private void UnregisterPrefabHandler()
        {
            var memberNetObj = _memberPrefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.RemoveHandler(memberNetObj);
        }

        private void AddMember(ulong clientId)
        {
            int? memIdx = _roomStatus.GetIndexById(clientId);
            bool isNew = !memIdx.HasValue;
           
            if (isNew) memIdx = _roomStatus.InsertMember(clientId);

            var newMember = SpawnNewMember(clientId, memIdx.Value);
            
            _members.Add(clientId, newMember);
            CheckStartState();
        }

        private void RemoveTargetMember(ulong targetId) // PlayerPrefab이라 자동으로 디스폰되는 바람에 직접 디스폰은 안하지만...
        {
            bool removed = _roomStatus.RemoveMember(targetId);

            if (removed)
            {
                _members.Remove(targetId);
                CheckStartState();
            }
        }
        
        public bool SwapMember(int idx1, int idx2)
        {
            if (!IsServer) return false;
            
            var swapped = _roomStatus.SwapMember(idx1, idx2);

            if (swapped)
            {
                var id1 = _roomStatus.GetIdByIndex(idx1);
                var id2 = _roomStatus.GetIdByIndex(idx2);

                if (id1.HasValue && _members.TryGetValue(id1.Value, out var mem1))
                {
                    mem1.InitReadyStateRpc();
                }

                if (id2.HasValue && _members.TryGetValue(id2.Value, out var mem2))
                {
                    mem2.InitReadyStateRpc();
                }
            }
            
            return swapped;
        }
        
        private void InitReadyStates(SwitchModeRequest req)
        {
            if (!IsServer) return;
            
            foreach (RoomMember mem in _members.Values)
            {
                if (mem == null || mem.IsHostMember) continue;
                mem.InitReadyStateRpc();
            }
            
            CheckStartState();
        }

        private void CheckStartState(SwitchReadyRespond res = default)
        {
            if (!IsServer) return;
            
            var msg = new SwitchStartMessage(this.CanStartStage);
            _startSwitchPub.Publish(msg);
        }
    }
}