using _Scripts.Shared._Data;
using _Scripts.Stage;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Shared.Sound
{
    public class SfxProvider : IProvider, IInitializable
    {
        private readonly SoundSettingsData _soundSettings;
        private readonly IObjectResolver _resolver;
        
        private IObjectPool<SfxBuilder> _pool;

        public SfxProvider(
                SoundSettingsData soundSettings,
                IObjectResolver resolver)
        {
            _soundSettings = soundSettings;
            _resolver = resolver;
        }

        public void Initialize()
        {
            InitPool();
        }

        public void InitPool()
        {
            _pool = new ObjectPool<SfxBuilder>(
                CreateSfx,
                OnGet,
                OnRelease,
                OnDestroySfx,
                defaultCapacity: _soundSettings.ProviderInfo.DefaultCount,
                maxSize: _soundSettings.ProviderInfo.MaxCount);

            for (int i = 0; i < _soundSettings.ProviderInfo.DefaultCount; i++)
            {
                Release(CreateSfx());
            }
        }

        private SfxBuilder CreateSfx()
        {
            var sfx = _resolver.Instantiate(_soundSettings.ProviderInfo.Prefab);
            sfx.name = $"{_soundSettings.ProviderInfo.ObjNamePrefix}_{sfx.GetHashCode()}";
            return sfx;
        }

        private void OnGet(SfxBuilder sfx)
        {
            sfx.Apply(_soundSettings.SfxVolume, _soundSettings.IsSfxMute);
            sfx.gameObject.SetActive(true);
        }

        private void OnRelease(SfxBuilder sfx)
        {
            sfx.gameObject.SetActive(false);
            sfx.Init();
        }
        
        private void OnDestroySfx(SfxBuilder sfx) { }

        public SfxBuilder Get()
        {
            return _pool.Get();
        }

        public void Release(SfxBuilder sfx)
        {
            _pool.Release(sfx);
        }
    }
}