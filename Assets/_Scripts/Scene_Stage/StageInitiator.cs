using System;
using System.Collections.Generic;
using _Scripts._Shared;
using _Scripts.Scene_Room.Data;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace _Scripts.Scene_Stage
{
    public class StageInitiator : IInitializable, IDisposable
    {
        private readonly NetworkManager _netManager;
        private readonly SceneChanger _sceneChanger;
        private readonly RoomStatus _room;

        public StageInitiator(
            NetworkManager networkManager,
            SceneChanger sceneChanger,
            RoomStatus roomStatus)
        {
            _netManager = networkManager;
            _sceneChanger = sceneChanger;
            _room = roomStatus;
        }
      
        public void Initialize()
        {
            var nsm = _netManager.SceneManager;
            
            nsm.OnLoadComplete += OnLocalCompleted;
            nsm.OnLoadEventCompleted += OnAllClientsCompleted;
        }

        public void Dispose()
        {
            var nsm = _netManager.SceneManager;
            if(nsm == null) return;
            
            nsm.OnLoadComplete -= OnLocalCompleted;
            nsm.OnLoadEventCompleted -= OnAllClientsCompleted;
        }
        
        private void OnLocalCompleted(
            ulong clientId,
            string sceneName,
            LoadSceneMode loadSceneMode)
        {
            
        }
        
        private void OnAllClientsCompleted(
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (!_netManager.IsServer) return;

            switch (sceneName)
            {
                case "Stage":
                    var uiSceneName = $"StageUi_{_room.CurStageData.Mode}";
                    _sceneChanger.LoadByServer(uiSceneName, LoadSceneMode.Additive);
                    break;
                case "StageUi_Comp" or "StageUi_Coop":
                    var levelSceneName = $"Level_{_room.CurStageData.Mode}_{_room.CurStageData.Id}";
                    _sceneChanger.LoadByServer(levelSceneName, LoadSceneMode.Additive);
                    break;
                case "Level":
                    break;
            }
        }
    }
}