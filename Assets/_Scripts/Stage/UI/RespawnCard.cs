using _Scripts._Helper;
using _Scripts.Stage._Data.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI
{
    public class RespawnCard : MonoBehaviour, INetworkUpdateSystem
    {
        [SF] private RectTransform rectTr;
        [SF] private TextMeshProUGUI timerTxt;
        
        private NetworkManager _netManager;
        private TweenHandler _tweenHandler;
        private RespawnCardData _data;
        
        private float _despawnTime;
        private int _prevSecond;

        [Inject]
        private void Construct(
            NetworkManager netManager,
            TweenHandler tweenHandler,
            RespawnCardData data)
        {
            _netManager = netManager;
            _tweenHandler = tweenHandler;
            _data = data;

            Quaternion camRot = Camera.main.transform.rotation;
            this.rectTr.rotation = camRot; 
        }

        private void OnDisable()
        {
            this.UnregisterNetworkUpdate();
        }

        public RespawnCard SetPos(Vector3 pos)
        {
            transform.position = pos + _data.Offset;
            return this;
        }

        public void Activate(float despawnTime)
        {
            _despawnTime = despawnTime;
            _prevSecond = int.MaxValue;
         
            this.gameObject.SetActive(true);
            
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        public void Deactivate()
        {
            this.gameObject.SetActive(false);
            
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void UpdateText(int remainingSecond)
        {
            if (_prevSecond <= remainingSecond) return;
            
            _prevSecond = remainingSecond;
            
            timerTxt.SetText(_data.TimerTextFormat, remainingSecond);
            
            _tweenHandler.Shake(rectTr, _data.ScaleSettings, _data.RotSettings);
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            var elapsedTime = _netManager.ServerTime.TimeAsFloat - _despawnTime;
            var remainingTime = _data.RespawnWaitTime - elapsedTime;

            if (remainingTime <= 0)
            {
                Deactivate();
                return;
            }
            
            UpdateText((int)remainingTime + 1);
        }
    }
}