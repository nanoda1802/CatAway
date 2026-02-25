using _Scripts.Stage.Table;
using _Scripts.Stage.UI.Widget;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Stage
{
    public class StageScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<StageHub>().AsSelf();
            
            builder.Register<PlacementBroker>(Lifetime.Scoped);
            builder.Register<ContactBroker>(Lifetime.Scoped);
            
            var msgOptions = builder.RegisterMessagePipe();
            
            builder.RegisterMessageBroker<IPlacable>(msgOptions);
            builder.RegisterMessageBroker<IProvider>(msgOptions);
            builder.RegisterMessageBroker<PublishRequestMessage>(msgOptions);
            
            base.Configure(builder);
        }
    }
}