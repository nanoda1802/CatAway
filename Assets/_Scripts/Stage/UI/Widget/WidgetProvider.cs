using _Scripts.Stage.UI.Movable;
using MessagePipe;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;


namespace _Scripts.Stage.UI.Widget
{
    public class WidgetProvider<T> : MonoBehaviour where T : WidgetBase
    {
        private IObjectResolver _container;
        private Canvas _canvas;
        private T _prefab;
        private WidgetData<T> _data;
        
        private IObjectPool<T> _pool;

        [Inject]
        private void ConstructBase(
            IObjectResolver container,
            Canvas canvas,
            T prefab,
            WidgetData<T> data)
        {
            _container = container;
            _canvas = canvas;
            _prefab = prefab;
            _data = data;
            
            InitPool();
        }
        
        private void InitPool()
        {
            _pool = new ObjectPool<T>(
                CreateWidget
                , OnGetWidget
                , OnReleaseWidget
                , OnDestroyWidget,
                true,
                _data.DefaultCount,
                _data.MaxCount);

            for (int i = 0; i < _data.DefaultCount; i++)
            {
                var widget = CreateWidget();
                ReleaseWidget(widget);
            }
        }
        
        private T CreateWidget()
        {
            var widget = _container.Instantiate(_prefab, _canvas.transform);
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