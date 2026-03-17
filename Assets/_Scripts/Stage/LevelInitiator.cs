using System;
using System.Collections.Generic;
using _Scripts.Lobby;
using _Scripts.Stage.Data;
using _Scripts.Stage.Player;
using _Scripts.Stage.Table.Contactable;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _Scripts.Stage
{
    public class LevelInitiator : IInitializable, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly StageData _stageData;
        private readonly NetworkManager _netManager;
        private readonly PlayerSyncer _playerPrefab;
        private readonly MemberInfo[] _memberInfos;

        private readonly Dictionary<uint, NetworkObject> _tablePrefabs = new();
        
        public LevelInitiator(
            IObjectResolver resolver,
            StageData stageData,
            NetworkManager netManager,
            PlayerSyncer playerPrefab,
            MemberInfo[] memberInfos)
        {
            _resolver = resolver;
            _stageData = stageData;
            _netManager = netManager;
            _playerPrefab = playerPrefab;
            _memberInfos = memberInfos;
        }

        public void Initialize()
        {
            CacheTablePrefabs();
            
            if (_netManager.IsServer) _netManager.SceneManager.OnLoadEventCompleted += OnLevelLoaded;
        }

        public void Dispose()
        {
            foreach (var prefab in _tablePrefabs.Values)
            {
                _netManager.PrefabHandler.RemoveHandler(prefab);
            }
        }
        
        private void OnLevelLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            Debug.Log($"[LevelInitiator.OnLevelLoaded] : {sceneName}");
            if (!_netManager.IsServer) return;
            if (!sceneName.Equals("Level")) return;
            
            SpawnTables();
            SpawnPlayers();
            
            _netManager.SceneManager.OnLoadEventCompleted -= OnLevelLoaded;
        }

        // 우선 스테이지 데이터 기준으로
        // 적절한 레벨 프리펩 생성 (컨테이너 통해서)
        
        // 접시 프로바이더에서 접시 꺼내서 랜덤 테이블에 올려놓기
        // 팬 프로바이더에서 팬 꺼내서 랜덤 테이블에 올려놓기

        private Dictionary<uint, NetworkObject> CacheTablePrefabs()
        {
            Debug.Log($"[LevelInitiator] Cache table prefabs");
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
            Debug.Log($"[LevelInitiator] Spawned tables");
            
            foreach (var info in _stageData.TableSpawnInfos)
            {
                if (!_tablePrefabs.TryGetValue(info.GlobalObjectHashId, out var prefab)) continue;
                
                var table = _resolver.Instantiate(prefab, info.Position, info.Rotation);

                if (info.IsPantry && table.TryGetComponent<PantryTable>(out var pantry))
                {
                    pantry.SetAs(info.PantryType);
                }

                table.Spawn(true);
            }
        }
        
        private void SpawnPlayers()
        {
            Debug.Log($"[LevelInitiator] Spawned players");
            for (int i = 0; i < _memberInfos.Length; i++)
            {
                var info = _memberInfos[i];
                if (info == null) continue;
                
                var spawnPoint = _stageData.PlayerSpawnPoints[i];
                var player = Object.Instantiate(_playerPrefab, spawnPoint, Quaternion.identity);
                player.SetAvatar(info.AvatarIndex);
                player.NetworkObject.SpawnAsPlayerObject(info.ClientId);
                
                Debug.Log($"player_{i} spawn : id? {info.ClientId} / avatar? {info.AvatarIndex}");
            }
        }
    }
}