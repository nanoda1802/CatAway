using _Scripts.Lobby;
using _Scripts.Messages;
using _Scripts.Messages.Stage;
using _Scripts.Stage.Data;
using _Scripts.Stage.Player;
using _Scripts.Stage.Table;
using _Scripts.Stage.UI.Board.Order;
using _Scripts.Stage.UI.Board.Score;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Application = UnityEngine.Device.Application;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage
{
    public class StageScope : LifetimeScope
    {
        [SF] private StageData stageData;
        [SF] private ProviderData providerData;
        [SF] private PlayerSyncer playerPrefab;
        
        private NetworkManager _netManager;
        private MemberInfo[] _memberInfos;
        
        protected override void Awake()
        {
            Application.targetFrameRate = 60;
            
            this.autoRun = false;
            
            base.Awake();
        }
        
        // autorun 끄기!!!!!!!!!!!!!!!
        
        public void BuildScopeWith(
            StageData data,
            NetworkManager netManager,
            MemberInfo[] memberInfos)
        {
            this.stageData = data;
            _netManager = netManager;
            _memberInfos = memberInfos;
            this.Build();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_netManager);
            
            builder.RegisterEntryPoint<StageHub>(Lifetime.Singleton).AsSelf();
            
            builder.Register<PlacementBroker>(Lifetime.Scoped);
            builder.Register<ContactBroker>(Lifetime.Scoped);
            
            builder.RegisterInstance(stageData);
            builder.RegisterInstance(providerData);
            builder.RegisterInstance(playerPrefab);
            builder.RegisterInstance(_memberInfos);
            
            var msgOptions = builder.RegisterMessagePipe();
            
            builder.RegisterMessageBroker<IPlacable>(msgOptions);
            builder.RegisterMessageBroker<IProvider>(msgOptions);
            builder.RegisterMessageBroker<OrderPresenter>(msgOptions);
            builder.RegisterMessageBroker<ScorePresenter>(msgOptions);
            builder.RegisterMessageBroker<PublishRequestMessage>(msgOptions);
            
            builder.RegisterMessageBroker<ScoreMessage>(msgOptions);
            builder.RegisterMessageBroker<AddOrderMessage>(msgOptions);
            builder.RegisterMessageBroker<RemoveOrderMessage>(msgOptions);
            builder.RegisterMessageBroker<float>(msgOptions); // Timer
            
            builder.RegisterMessageBroker<PopUpMessage>(msgOptions);
            builder.RegisterMessageBroker<PopDownMessage>(msgOptions);
            
            builder.RegisterMessageBroker<CueMessage>(msgOptions);
            
            builder.RegisterMessageBroker<StartStageMessage>(msgOptions);
            builder.RegisterMessageBroker<EndStageMessage>(msgOptions);
            
            base.Configure(builder);
        }
    }
}