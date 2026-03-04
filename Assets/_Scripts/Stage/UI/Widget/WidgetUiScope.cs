using _Scripts.Stage.UI.Widget.PlatingIcon;
using _Scripts.Stage.UI.Widget.ProgressBar;
using _Scripts.Stage.UI.Widget.TableAlert;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Widget
{
    public class WidgetUiScope : LifetimeScope
    {
        private Canvas _widgetCanvas;
        private RectTransform _canvasRectTr;
        private Camera _mainCam;
        
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
            
            base.Configure(builder);
        }
    }
}