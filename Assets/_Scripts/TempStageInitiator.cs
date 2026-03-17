using System;
using _Scripts.Stage;
using _Scripts.Stage.Data;
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

            timerPresenter ??= FindAnyObjectByType<TimerPresenter>();

            foreach (var op in FindObjectsByType<OrderPresenter>(FindObjectsSortMode.None))
            {
                if (op.Team == Team.Red)
                {
                    orderPresenterRed = op;
                }
                else
                {
                    orderPresenterBlue = op;
                }
            }
            
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