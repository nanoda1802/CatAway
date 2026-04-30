using System.Collections.Generic;
using _Scripts.Result._Messages;
using MessagePipe;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace _Scripts.Result
{
    public class ResultInitiator : IInitializable
    {
        private readonly NetworkManager _netManager;

        private readonly IPublisher<StartResultMessage> _startPub;

        public ResultInitiator(
            NetworkManager netManager,
            IPublisher<StartResultMessage> startPub)
        {
            _netManager = netManager;
            _startPub = startPub;
        }

        public void Initialize()
        {
            _netManager.SceneManager.OnLoadEventCompleted += OnAllClientsCompleted;
        }
        
        private void OnAllClientsCompleted(
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (!_netManager.IsServer) return;
            if (sceneName != "Result") return;

            var msg = new StartResultMessage();
            _startPub.Publish(msg);
            
            _netManager.SceneManager.OnLoadEventCompleted -= OnAllClientsCompleted;
        }
    }
}