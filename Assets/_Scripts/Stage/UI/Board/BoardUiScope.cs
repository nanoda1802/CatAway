using _Scripts.Stage.UI.Board.Order;
using _Scripts.Stage.UI.Board.Score;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board
{
    public class BoardUiScope : LifetimeScope
    {
        private Canvas _boardCanvas;
        private RectTransform _canvasRectTr;
        
        [Header("[ Data ]")]
        [SF] private BoardUiData boardUiData;
        [SF] private OrderCardData orderCardData;
        [Header("[ Prefab ]")]
        [SF] private OrderCard orderCardPrefab;
        
        protected override void Awake()
        {
            _boardCanvas = GetComponent<Canvas>();
            _canvasRectTr = GetComponent<RectTransform>();

            if (this.autoInjectGameObjects.Count <= 0)
            {
                autoInjectGameObjects.Add(this.gameObject);
            }

            if (!this.autoRun)
            {
                this.autoRun = true;
            }

            base.Awake();
        }
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_boardCanvas);
            builder.RegisterComponent(_canvasRectTr);

            builder.RegisterInstance(boardUiData);
            builder.RegisterInstance(orderCardData);
            builder.RegisterInstance(orderCardPrefab);

            // var options = this.Parent.Container.Resolve<MessagePipeOptions>();
            // builder.RegisterMessageBroker<ScorePacket>(options);
            
            base.Configure(builder);
        }
    }
}