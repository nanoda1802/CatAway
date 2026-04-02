using System;
using System.Collections.Generic;
using _Scripts._Shared;
using _Scripts._Shared.Sound;
using _Scripts.Messages.Stage;
using _Scripts.Scene_Room.Data;
using MessagePipe;
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
        private readonly SoundManager _soundManager;

        public StageInitiator(
            NetworkManager networkManager,
            SceneChanger sceneChanger,
            RoomStatus roomStatus,
            SoundManager soundManager,
            ISubscriber<StartStageMessage> startSub,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _netManager = networkManager;
            _sceneChanger = sceneChanger;
            _room = roomStatus;
            _soundManager = soundManager;
            
            startSub
                .Subscribe(StartBgm)
                .AddTo(disposableBagBuilder);
            
            endSub
                .Subscribe(StopBgm)
                .AddTo(disposableBagBuilder);
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

        private void StartBgm(StartStageMessage msg)
        {
            _soundManager.PlayBgm(_room.CurStageData.Bgm).Forget();
        }

        private void StopBgm(EndStageMessage msg)
        {
            _soundManager.StopBgm().Forget();
        }
    }
}