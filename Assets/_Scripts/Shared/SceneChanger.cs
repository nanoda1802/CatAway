using System;
using System.Collections.Generic;
using _Scripts.Shared._Messages;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace _Scripts.Shared
{
    public class SceneChanger : IInitializable, IDisposable 
    {
        private readonly NetworkManager _netManager;
        
        private bool InNetwork => _netManager != null && _netManager.IsListening;
        
        public SceneChanger(
            NetworkManager networkManager,
            DisposableBagBuilder rootDisposableBagBuilder,
            ISubscriber<LoadSceneMessage> loadSub)
        {
            _netManager = networkManager;
            
            loadSub
                .Subscribe(HandleMessage)
                .AddTo(rootDisposableBagBuilder);
        }

        public void Initialize()
        {
            _netManager.OnClientStarted += SubscribeSceneEvents;
            _netManager.OnPreShutdown += UnsubscribeSceneEvents;
        }

        public void Dispose()
        {
            _netManager.OnClientStarted -= SubscribeSceneEvents;
            _netManager.OnPreShutdown -= UnsubscribeSceneEvents;
        }

        private void SubscribeSceneEvents()
        {
            var nsm = _netManager.SceneManager;
            
            nsm.OnLoad += OnLoadStarted;
            nsm.OnLoadComplete += OnLocalCompleted;
            nsm.OnLoadEventCompleted += OnAllClientsCompleted;
        }
        
        private void UnsubscribeSceneEvents()
        {
            var nsm = _netManager.SceneManager;
            if (nsm == null) return;
            
            nsm.OnLoad -= OnLoadStarted;
            nsm.OnLoadComplete -= OnLocalCompleted;
            nsm.OnLoadEventCompleted -= OnAllClientsCompleted;
        }

        private void HandleMessage(LoadSceneMessage msg)
        {
            if (InNetwork)
            {
                LoadByServer(msg.SceneName, msg.LoadMode);
                return;
            }

            LoadSelf(msg.SceneName, msg.LoadMode);
        }

        private void LoadSelf(string sceneName, LoadSceneMode mode)
        {
            SceneManager.LoadScene(sceneName, mode);
        }

        public void LoadByServer(string sceneName, LoadSceneMode mode)
        {
            if (!_netManager.IsServer) return;

            var loadStatus = _netManager.SceneManager.LoadScene(sceneName, mode);

            if (loadStatus != SceneEventProgressStatus.Started)
            {
                Debug.LogError($"[{sceneName} 씬 로드 실패] {loadStatus}");
                return;
            }
        }

        private void OnLoadStarted(
            ulong clientId,
            string sceneName,
            LoadSceneMode loadSceneMode,
            AsyncOperation asyncOperation)
        {
            Debug.LogWarning($"[{clientId}의 {sceneName} 씬 로드 시작]");
        }

        private void OnLocalCompleted(
            ulong clientId,
            string sceneName,
            LoadSceneMode loadSceneMode)
        {
            Debug.LogWarning($"[{clientId}의 {sceneName} 씬 로드 완료]");
        }
        
        private void OnAllClientsCompleted(
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (_netManager.IsServer)
            {
                Debug.LogWarning($"[모든 참여자의 {sceneName} 씬 로드 완료]");
            }
        }
    }
}