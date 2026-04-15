using System.Collections.Generic;
using System.Threading;
using _Scripts.Shared._Messages;
using _Scripts.Stage._Enums;
using _Scripts.Stage._Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Pop
{
    public class CuePresenter : NetworkBehaviour
    {
        [SF] private float startCueDuration = 3f;
        [SF] private float endCueDuration = 1.75f;

        private IPublisher<StartStageMessage> _startPub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<PopDownMessage> _popDownPub;
        private IPublisher<CueMessage> _cuePub;
        private IPublisher<LoadSceneMessage> _loadScenePub;

        private CancellationTokenSource _cts;

        [Inject]
        private void Construct(
            IPublisher<StartStageMessage> startPub,
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<PopDownMessage> popDownPub,
            IPublisher<CueMessage> cuePub,
            IPublisher<LoadSceneMessage> loadScenePub,
            IPublisher<CuePresenter> presenterPub,
            IBufferedSubscriber<HubCallMessage> requestSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _startPub = startPub;
            _popUpPub = popUpPub;
            _popDownPub = popDownPub;
            _cuePub = cuePub;
            _loadScenePub = loadScenePub;
            
            presenterPub.Publish(this);
            
            requestSub
                .Subscribe(msg =>
                {
                    if (!msg.IsRequest(this)) return;
                    presenterPub.Publish(this);
                    
                }).AddTo(disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) NetworkManager.SceneManager.OnLoadEventCompleted += OnLevelLoaded;
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            base.OnNetworkDespawn();
        }

        private void OnLevelLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!IsServer) return;
            if (!sceneName.StartsWith("Level")) return;

            DisplayCue(CueType.Start).Forget();
            
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLevelLoaded;
        }

        public async UniTaskVoid DisplayCue(CueType cueType)
        {
            var token = RefreshToken();
            var duration = cueType == CueType.Start ? startCueDuration : endCueDuration;
            
            PopUpCueRpc(cueType, duration);
            
            await UniTask.Delay((int)(duration * 1000), cancellationToken:token);

            PopDownCueRpc(cueType);
        }

        [Rpc(SendTo.Everyone)]
        private void PopUpCueRpc(CueType cueType, float duration)
        {
            var cueMsg = new CueMessage(cueType, duration);
            var popUpMsg = new PopUpMessage(typeof(CuePop));
            
            _cuePub.Publish(cueMsg);
            _popUpPub.Publish(popUpMsg);
        }
        
        [Rpc(SendTo.Everyone)]
        private void PopDownCueRpc(CueType cueType)
        {
            var popDownMsg = new PopDownMessage();
            _popDownPub.Publish(popDownMsg);
            
            switch (cueType)
            {
                case CueType.End:
                    var loadMsg = new LoadSceneMessage("Result", LoadSceneMode.Single);
                    _loadScenePub.Publish(loadMsg);
                    break;
                case CueType.Start:
                    var startMsg = new StartStageMessage();
                    _startPub.Publish(startMsg);
                    break;
                default:
                    break;
            }
        }

        private CancellationToken RefreshToken()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }
}