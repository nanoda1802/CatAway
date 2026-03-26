using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.UI.Pop
{
    public class PopScope : LifetimeScope
    {
        [SF] private CanvasGroup canvasGroup;
        [SF] private PopPanel popPanel;
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(canvasGroup);
            builder.RegisterComponent(popPanel);
            
            builder.RegisterInstance(_disposableBagBuilder);
            
            base.Configure(builder);
        }

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            
            base.OnDestroy();
        }
    }
}