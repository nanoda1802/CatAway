using System;
using System.Collections.Generic;
using _Scripts.Lobby.Room;
using _Scripts.Messages.Room;
using _Scripts.Stage;
using _Scripts.Stage.Data;
using _Scripts.Stage.Player;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace _Scripts.Lobby
{
    public class StageLoader : NetworkBehaviour
    {
        private RoomSyncer _roomSyncer;
        
        private MemberInfo[] _memberInfos;
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();

        [Inject]
        private void Construct(
            RoomSyncer roomSyncer,
            ISubscriber<LoadStageMessage> startStageSub)
        {
            _roomSyncer = roomSyncer;
            
            startStageSub
                .Subscribe(LoadStage)
                .AddTo(_disposableBagBuilder);
        }
        
        private void Awake()
        {
            // var sm = NetworkManager.SceneManager;
            //
            // var a = sm.ActiveSceneSynchronizationEnabled;
            // var b = sm.ClientSynchronizationMode;
            //
            // var c = sm.PostSynchronizationSceneUnloading;
            // var d = sm.VerifySceneBeforeLoading;
            // var e = sm.VerifySceneBeforeUnloading;
            //
            // sm.DisableValidationWarnings();
            // sm.GetSceneMapping();
            // sm.GetSynchronizedScenes();
            // sm.LoadScene();
            // sm.UnloadScene()
            // sm.SetClientSynchronizationMode();
            //
            // sm.OnSceneEvent;
            //
            // sm.OnLoad;
            // sm.OnUnload;
            // sm.OnLoadEventCompleted;
            // sm.OnUnloadEventCompleted;
            // sm.OnLoadComplete;
            // sm.OnUnloadComplete;
            // sm.OnSynchronize;
            // sm.OnSynchronizeComplete;
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.SceneManager.OnLoad += OnLoadStarted;
            NetworkManager.SceneManager.OnLoadComplete += OnLocalComplete;
            NetworkManager.SceneManager.OnLoadEventCompleted += OnAllCompleted;
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.SceneManager.OnLoad -= OnLoadStarted;
            NetworkManager.SceneManager.OnLoadComplete -= OnLocalComplete;
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnAllCompleted;
            
            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            
            base.OnDestroy();
        }

        private void OnLoadStarted(ulong clientId, string sceneName, LoadSceneMode loadSceneMode,
            AsyncOperation asyncOperation)
        {
            Debug.Log($"[OnLoadStarted] id? {clientId} / scene? {sceneName}");
            LogProgress(sceneName,asyncOperation).Forget();
        }

        private async UniTaskVoid LogProgress(string sceneName, AsyncOperation loadOper)
        {
            while (!loadOper.isDone)
            {
                Debug.Log($"[{(int)loadOper.progress*100:D2}%] {sceneName} is loading... ");
                await UniTask.Yield();
            }
        }

        private void OnLocalComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            Debug.Log($"[OnLoadComplete] id? {clientId} / scene? {sceneName}");
            switch (sceneName)
            {
                case "Stage":
                    InjectDataToScope();
                    break;
            }
        }
        
        private void OnAllCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!IsServer) return;
            Debug.Log($"[OnLoadEventCompleted] scene? {sceneName} / clientCount? {clientsCompleted.Count} / timedOut? {clientsTimedOut.Count}");
            
            if (sceneName.Equals("Stage")) LoadUi();
            if (sceneName.StartsWith("StageUi")) LoadLevel();
        }
        
        private void LoadStage(LoadStageMessage msg)
        {
            if (!IsServer) return;
            if (!_roomSyncer.CanStartStage) return;
            
            _memberInfos = null;
            
            Debug.Log($"[LoadStage] Try to load stage");
            var status = NetworkManager.SceneManager.LoadScene("Stage", LoadSceneMode.Single);
            
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.Log($"[LoadStage] Failed to load : {status}");
                return;
            }
            
            _memberInfos = _roomSyncer.MemberInfos;
        }

        private void LoadUi()
        {
            Debug.Log($"[LoadUi] Try to load stageUi");
            
            var sceneName = $"StageUi_{_roomSyncer.CurMode}";
            
            var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.Log($"[LoadUi] Failed to load : {status}]");
                return;
            }
        }

        private void LoadLevel()
        {
            Debug.Log($"[LoadLevel] Try to load level");
            
            var sceneName = "Level";
            
            var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.Log($"[LoadLevel] Failed to load : {status}]");
                return;
            }
        }

        private void InjectDataToScope()
        {
            var stageScope = FindFirstObjectByType<StageScope>();
            var data = _roomSyncer.CurStageData;
            
            _memberInfos ??= new MemberInfo[1]; // [임시] 클라인 경우...
            
            Debug.Log($"[InjectToStageScope] scope? {stageScope != null} / data? {data.Mode}_{data.Id} / memberCount {_memberInfos?.Length}");
            
            stageScope.BuildScopeWith(data, this.NetworkManager, _memberInfos);
        }
    }
}