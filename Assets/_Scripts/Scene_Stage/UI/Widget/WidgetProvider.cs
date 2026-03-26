using _Scripts.Scene_Stage.Data.Level;
using _Scripts.Stage;
using MessagePipe;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Scene_Stage.UI.Widget
{
    public class WidgetProvider<T> : MonoBehaviour, IProvider where T : WidgetBase
    {
        private IObjectResolver _resolver;
        private Canvas _canvas;

        private ProviderInfo<T> _info;
        
        private IObjectPool<T> _pool;
        
        [Inject]
        private void ConstructBase(
            IObjectResolver container,
            Canvas canvas,
            ProviderData providerData,
            IPublisher<IProvider> pub,
            IBufferedSubscriber<HubCallMessage> sub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _resolver = container;
            _canvas = canvas;
            
            _info = providerData.GetWidgetProviderInfo<T>();
            
            InitPool();

            pub.Publish(this);
            
            sub.Subscribe(msg =>
                {
                    if (!msg.IsRequest(this)) return;
                    pub.Publish(this);
                })
                .AddTo(disposableBagBuilder);
        }
        
        public void InitPool()
        {
            _pool = new ObjectPool<T>(
                CreateWidget
                , OnGetWidget
                , OnReleaseWidget
                , OnDestroyWidget,
                true,
                _info.DefaultCount,
                _info.MaxCount);

            for (int i = 0; i < _info.DefaultCount; i++)
            {
                var widget = CreateWidget();
                ReleaseWidget(widget);
            }
        }
        
        private T CreateWidget()
        {
            var widget = _resolver.Instantiate(_info.Prefab, _canvas.transform);
            return widget;
        }

        private void OnGetWidget(T widget)
        {
            widget.Show();
        }

        private void OnReleaseWidget(T widget)
        {
            widget.Hide();
        }

        private void OnDestroyWidget(T widget)
        {
        }

        public T GetWidget(Vector3 worldPos)
        {
            var widget = _pool.Get();
            widget.UpdatePosition(worldPos);
            return widget;
        }

        public void ReleaseWidget(T widget)
        {
            _pool.Release(widget);
        }
    }
}