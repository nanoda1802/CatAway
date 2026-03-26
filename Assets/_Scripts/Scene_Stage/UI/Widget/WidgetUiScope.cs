using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Scene_Stage.UI.Widget
{
    public class WidgetUiScope : LifetimeScope
    {
        private Canvas _widgetCanvas;
        private RectTransform _canvasRectTr;
        private Camera _mainCam;
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        protected override void Awake()
        {
            _widgetCanvas = GetComponent<Canvas>();
            _canvasRectTr = GetComponent<RectTransform>();
            _mainCam = Camera.main;

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
            builder.RegisterComponent(_widgetCanvas);
            builder.RegisterComponent(_canvasRectTr);
            builder.RegisterComponent(_mainCam);
            
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