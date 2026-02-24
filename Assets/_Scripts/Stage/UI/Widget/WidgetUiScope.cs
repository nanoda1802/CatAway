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
        
        [Header("[ Data ]")]
        [SF] private ProgressBarData progressBarData;
        [SF] private TableAlertData tableAlertData;
        [SF] private PlatingIconData platingIconData;
        [Header("[ Prefab ]")]
        [SF] private ProgressBarWidget progressBarPrefab;
        [SF] private TableAlertWidget tableAlertPrefab;
        [SF] private PlatingIconWidget platingIconPrefab;
        
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

            builder.RegisterInstance(progressBarPrefab);
            builder.RegisterInstance(progressBarData)
                .As<WidgetData<ProgressBarWidget>>()
                .AsSelf();
            
            builder.RegisterInstance(tableAlertPrefab);
            builder.RegisterInstance(tableAlertData)
                .As<WidgetData<TableAlertWidget>>()
                .AsSelf();
            
            builder.RegisterInstance(platingIconPrefab);
            builder.RegisterInstance(platingIconData)
                .As<WidgetData<PlatingIconWidget>>()
                .AsSelf();
            
            base.Configure(builder);
        }
    }
}