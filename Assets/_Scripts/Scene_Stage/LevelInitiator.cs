using System;
using System.Collections.Generic;
using _Scripts.Messages.Stage;
using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Player;
using _Scripts.Scene_Stage.Table;
using _Scripts.Scene_Stage.Table.Contactable;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _Scripts.Scene_Stage
{
    public class LevelInitiator : IInitializable, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly NetworkManager _netManager;
        private readonly RoomStatus _room;
        private readonly PlayerSyncer _playerPrefab;

        private readonly Dictionary<uint, NetworkObject> _tablePrefabs = new();
        
        public LevelInitiator(
            IObjectResolver resolver,
            NetworkManager netManager,
            RoomStatus room,
            PlayerSyncer playerPrefab)
        {
            _resolver = resolver;
            _netManager = netManager;
            _room = room;
            _playerPrefab = playerPrefab;
        }

        public void Initialize()
        {
            CacheTablePrefabs();

            var playerNetObj = _playerPrefab.GetComponent<NetworkObject>();
            _netManager.PrefabHandler.AddHandler(playerNetObj, new PlayerPrefabHandler(this));
            
            if (_netManager.IsServer) _netManager.SceneManager.OnLoadEventCompleted += OnLevelLoaded;
        }

        public void Dispose()
        {
            var playerNetObj = _playerPrefab.GetComponent<NetworkObject>();
            _netManager.PrefabHandler.RemoveHandler(playerNetObj);
            
            foreach (var prefab in _tablePrefabs.Values)
            {
                _netManager.PrefabHandler.RemoveHandler(prefab);
            }
        }
        
        private void OnLevelLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!_netManager.IsServer) return;
            if (!sceneName.StartsWith("Level")) return;
            
            // SpawnTables();
            SpawnPlayers();
            
            _netManager.SceneManager.OnLoadEventCompleted -= OnLevelLoaded;
        }

        private Dictionary<uint, NetworkObject> CacheTablePrefabs()
        {
            var networkPrefabs = _netManager.NetworkConfig.Prefabs.Prefabs;
            
            foreach (var networkPrefab in networkPrefabs)
            {
                if (networkPrefab == null) continue;
                if (!networkPrefab.Prefab.CompareTag("Table")) continue;
                if (!networkPrefab.Prefab.TryGetComponent<NetworkObject>(out var netObj)) continue;
                
                _tablePrefabs.Add(netObj.PrefabIdHash, netObj);
                
                _netManager.PrefabHandler.AddHandler(netObj, new TablePrefabHandler(_resolver, netObj));
            }
            
            return _tablePrefabs;
        }

        private void SpawnTables()
        {
            foreach (var info in _room.CurStageData.TableSpawnInfos)
            {
                if (!_tablePrefabs.TryGetValue(info.GlobalObjectHashId, out var prefab)) continue;
                
                var table = _resolver.Instantiate(prefab, info.Position, info.Rotation);

                if (info.IsPantry && table.TryGetComponent<PantryTable_BackUp>(out var pantry))
                {
                    pantry.SetAs(info.PantryType);
                }

                table.Spawn(true);
            }
        }

        public PlayerSyncer CreatePlayer(Vector3 pos)
        {
            var player = Object.Instantiate(_playerPrefab, pos, Quaternion.identity);
            return player;
        }

        private void SpawnPlayers()
        {
            var members = _room.Members;
            if (members is not { Length: > 0 }) return;
            
            for (int i = 0; i < members.Length; i++)
            {
                MemberInfo mem = members[i];
                if (mem == null) continue;
                
                Vector3 spawnPoint = _room.CurStageData.PlayerSpawnPoints[i];
                
                var player = CreatePlayer(spawnPoint).ApplySpawnInfo(false);
                
                _netManager.PrefabHandler.SetInstantiationData(player.NetObj, new PlayerSpawnPacket(false));
                player.NetObj.SpawnAsPlayerObject(mem.ClientId, true);
                
                // var player = Object.Instantiate(_playerPrefab, spawnPoint, Quaternion.identity);
                // player.NetworkObject.SpawnAsPlayerObject(mem.ClientId,true);
            }
        }
    }
}