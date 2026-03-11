using _Scripts.Stage.Table;
using _Scripts.Stage.UI.Board.Order;
using _Scripts.Stage.UI.Board.Score;
using MessagePipe;
using UnityEngine.Device;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage
{
    public class StageScope : LifetimeScope
    {
        [SF] private StageData stageData;
        [SF] private ProviderData providerData;
        
        protected override void Awake()
        {
            Application.targetFrameRate = 60;
            
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<StageHub>(Lifetime.Singleton).AsSelf();
            
            builder.Register<PlacementBroker>(Lifetime.Scoped);
            builder.Register<ContactBroker>(Lifetime.Scoped);
            
            builder.RegisterInstance(stageData);
            builder.RegisterInstance(providerData);
            
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
            
            base.Configure(builder);
        }
    }
}