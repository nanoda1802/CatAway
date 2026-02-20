using _Scripts.Stage.UI.Movable;
using _Scripts.Stage.UI.Widget;
using _Scripts.Stage.UI.Widget.ProgressBar;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Stage
{
    public class StageScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var msgOptions = builder.RegisterMessagePipe();
            
            builder.RegisterMessageBroker<ProgressBarProvider>(msgOptions);
            // 다른 Provider들도 추가
            
            base.Configure(builder);
        }
    }
}