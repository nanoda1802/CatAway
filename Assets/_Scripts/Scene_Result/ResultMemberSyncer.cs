using System.Collections.Generic;
using System.Linq;
using _Scripts.Messages.StageResult;
using _Scripts.Scene_Room;
using _Scripts.Scene_Room.Data;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Result
{
    public class ResultMemberSyncer : NetworkBehaviour
    {
        [SF] private float posOffsetX = 2.0f;
        [SF] private float rotOffsetY = 15.0f;
        [SF] private float posZ = -5.5f;
        
        private IObjectResolver _resolver;
        private ResultMember _memberPrefab;
        private RoomStatus _roomStatus;
        
        private readonly Dictionary<ulong, ResultMember> _members = new();
        
        [Inject]
        private void Construct(
            IObjectResolver resolver,
            ResultMember memberPrefab,
            RoomStatus roomStatus)
        {
            _resolver = resolver;
            _memberPrefab = memberPrefab;
            _roomStatus = roomStatus;
        
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

        private void OnRoomLoadComplete(
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (!IsServer) return;
            if (sceneName != "Result") return;

            SpawnMembers(_roomStatus.ActiveMembers.ToList());
            
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnRoomLoadComplete;
        }
        
        public ResultMember CreateMemberObject(ulong clientId, Vector3 pos, Quaternion rot)
        {
            var memObj = _resolver.Instantiate(_memberPrefab, pos, rot);
            memObj.name = $"Member_{clientId}";
            return memObj;
        }
        
        private ResultMember SpawnNewMember(ulong clientId, Vector3 spawnPos, Quaternion spawnRot)
        {
            var newMem = CreateMemberObject(clientId, spawnPos, spawnRot);
        
            _members.Add(clientId, newMem);
            
            var netObj = newMem.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId,true);
            
            return newMem;
        }
        
        private void OnConnection(NetworkManager netMgr, ConnectionEventData eventData)
        {
            if (eventData.EventType != ConnectionEvent.ClientDisconnected) return;
            if (!IsServer) return;
            
            bool removed = _roomStatus.RemoveMember(eventData.ClientId, out int idx);
            
            if (removed)
            {
                _members.Remove(eventData.ClientId);
                RefreshMembers(_roomStatus.ActiveMembers.ToList());
            }
        }

        private void RegisterPrefabHandler()
        {
            var memberNetObj = _memberPrefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.AddHandler(memberNetObj, new ResultMemberPrefabHandler(this));
        }

        private void UnregisterPrefabHandler()
        {
            var memberNetObj = _memberPrefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.RemoveHandler(memberNetObj);
        }

        private (Vector3, Quaternion) CalculatePosAndRot(int memCount, int idx)
        {
            Vector3 pos = new Vector3((memCount - 1) - (idx * posOffsetX), 0, posZ);
            float rotY = (idx - (memCount - 1) / 2f) * rotOffsetY;
            Quaternion rot = Quaternion.Euler(0, rotY, 0);
            return (pos, rot);
        }

        private void SpawnMembers(List<MemberInfo> activeMem)
        {
            if (!IsServer) return;

            int memCount = activeMem.Count;

            for (int i = 0; i < memCount; i++)
            {
                (Vector3 spawnPos, Quaternion spawnRot) = CalculatePosAndRot(memCount, i);
                SpawnNewMember(activeMem[i].ClientId, spawnPos, spawnRot);
            }
        }

        private void RefreshMembers(List<MemberInfo> activeMem)
        {
            if (!IsServer) return;

            int memCount = _members.Count;

            for (int i = 0; i < memCount; i++)
            {
                if (!_members.TryGetValue(activeMem[i].ClientId, out var mem)) continue;
                
                (Vector3 newPos, Quaternion newRot) = CalculatePosAndRot(memCount, i);
                
                mem.RePosition(newPos, newRot);
            }
        }
    }
}