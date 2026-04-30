using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Shared.UI.Pop
{
    public class PopScope : LifetimeScope
    {
        [SF] private Canvas popCanvas;
        [SF] private CanvasGroup popGroup;
        [SF] private PopPanel popPanel;
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.UseComponents(componentsBuilder =>
            {
                componentsBuilder.AddInstance(popCanvas);
                componentsBuilder.AddInstance(popGroup);
                componentsBuilder.AddInstance(popPanel);
            });
            
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