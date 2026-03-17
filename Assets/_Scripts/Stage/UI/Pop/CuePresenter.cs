using System.Collections.Generic;
using _Scripts.Messages;
using _Scripts.Messages.Stage;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Pop
{
    public class CuePresenter : NetworkBehaviour
    {
        [SF] private float startCueDuration = 2f;
        [SF] private float endCueDuration = 1f;

        private IPublisher<StartStageMessage> _startPub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<PopDownMessage> _popDownPub;
        private IPublisher<CueMessage> _cuePub;
        
        [Inject]
        private void Construct(
            IPublisher<StartStageMessage> startPub,
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<PopDownMessage> popDownPub,
            IPublisher<CueMessage> cuePub)
        {
            _startPub = startPub;
            _popUpPub = popUpPub;
            _popDownPub = popDownPub;
            _cuePub = cuePub;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) NetworkManager.SceneManager.OnLoadEventCompleted += OnLevelLoaded;
            
            base.OnNetworkSpawn();
        }
        
        private void OnLevelLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!IsServer) return;
            if (!sceneName.Equals("Level")) return;
            Debug.Log($"[CuePresenter.OnLevelLoaded] : {sceneName}");

            DisplayCue(CueType.Start, startCueDuration).Forget();
            
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLevelLoaded;
        }

        private async UniTaskVoid DisplayCue(CueType cueType, float duration)
        {
            PopUpCueRpc(cueType, duration);
            
            await UniTask.Delay((int)(duration * 1000));

            PopDownCueRpc();
            
            var msg = new StartStageMessage();
            _startPub.Publish(msg);
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
        private void PopDownCueRpc()
        {
            var popDownMsg = new PopDownMessage();
            _popDownPub.Publish(popDownMsg);
        }
    }
}