using System;
using _Scripts.Stage.UI.Board.Order;
using _Scripts.Stage.UI.Board.Timer;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;

namespace _Scripts
{
    public class TempStageInitiator : MonoBehaviour
    {
        [SF] private Button btn;

        [SF] private TimerPresenter timerPresenter;
        [SF] private OrderPresenter orderPresenterBlue;
        [SF] private OrderPresenter orderPresenterRed;
        
        
        private void Start()
        {
            btn.onClick.AddListener(Initiate);
        }

        private void Initiate()
        {
            if (NetworkManager.Singleton == null) return;
            if (!NetworkManager.Singleton.IsServer) return;
            
            timerPresenter?.BeginTimer();
            orderPresenterBlue?.BeginOrder();
            orderPresenterRed?.BeginOrder();
        }
    }
}