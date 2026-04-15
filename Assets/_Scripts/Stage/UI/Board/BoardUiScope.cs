using _Scripts.Stage._Data.UI;
using _Scripts.Stage.UI.Board.Order;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board
{
    public class BoardUiScope : LifetimeScope
    {
        [Header("[ Data ]")]
        [SF] private BoardUiData boardUiData;
        [SF] private OrderCardData orderCardData;
        [SF] private RespawnCardData respawnCardData;

        [Header("[ Prefab ]")]
        [SF] private OrderCard orderCardPrefab;
        [SF] private RespawnCard respawnCardPrefab;
        
        private Canvas _boardCanvas;
        private RectTransform _canvasRectTr;
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
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
            
            builder.RegisterInstance(respawnCardData);
            builder.RegisterInstance(respawnCardPrefab);
            
            builder.RegisterInstance(_disposableBagBuilder);

            // var options = this.Parent.Container.Resolve<MessagePipeOptions>();
            // builder.RegisterMessageBroker<ScorePacket>(options);
            
            base.Configure(builder);
        }

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }
    }
}