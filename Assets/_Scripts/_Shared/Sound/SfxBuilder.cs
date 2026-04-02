using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace _Scripts._Shared.Sound
{
    public class SfxBuilder : MonoBehaviour
    {
        private AudioSource _audioSource;
        private SoundManager _soundManager;

        private CancellationTokenSource _cts;
        
        private AudioClip CurClip
        {
            get => _audioSource.clip;
            set => _audioSource.clip = value;
        }
        
        public bool IsLoop
        {
            get => _audioSource.loop;
            set => _audioSource.loop = value;
        }
        
        public float Volume
        {
            get => _audioSource.volume;
            set => _audioSource.volume = value;
        }

        public bool IsMute
        {
            get => _audioSource.mute;
            set => _audioSource.mute = value;
        }
        
        private bool HasClip => CurClip != null;

        [Inject]
        private void Construct(SoundManager soundManager)
        {
            if (!TryGetComponent(out _audioSource))
                _audioSource = gameObject.AddComponent<AudioSource>();
            
            _soundManager = soundManager;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void Init()
        {
            CurClip = null;
            IsLoop = false;
            _audioSource.pitch = 1;
            
            transform.SetParent(_soundManager.transform);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void Apply(float volume, bool isMute)
        {
            Volume = volume;
            IsMute = isMute;
        }

        public void TogglePause(bool isPaused)
        {
            if (isPaused)  _audioSource.Pause();
            else _audioSource.UnPause();
        }
        
        public void Stop()
        {
            _audioSource.Stop();
            _soundManager.ReleaseBuilder(this);
            Debug.Log($"<color=green>[SFX]</color> {this.GetType().Name} Stop");
        }

        public bool Play()
        {
            Debug.Log($"<color=green>[SFX]</color> {this.GetType().Name} Play : hasClip? {HasClip}");
            if (!HasClip) return false;
            
            _audioSource.Play();
            if (!IsLoop) Playing().Forget();
            
            return true;
        }

        private async UniTaskVoid Playing()
        {
            var token = RefreshToken();
            float clipLen = CurClip.length;
            
            Debug.Log($"<color=green>[SFX]</color> {this.GetType().Name} Playing : clipLen {clipLen}");
            
            while (clipLen > 0 && !token.IsCancellationRequested)
            {
                clipLen -= Time.unscaledDeltaTime;
                await UniTask.Yield(cancellationToken:token).SuppressCancellationThrow();
            }
            
            Stop();
        }

        public SfxBuilder WithInfo(AudioClip clip, bool isLoop = false)
        {
            CurClip = clip;
            IsLoop = isLoop;
            
            return this;
        }

        public SfxBuilder WithRandomPitch(bool randomPitch)
        {
            if (randomPitch) _audioSource.pitch += Random.Range(-0.05f, 0.05f);
            
            return this;
        }

        public SfxBuilder WithPos(Transform source)
        {
            if (source != null)
            {
                transform.SetParent(source);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);    
            }
            
            return this;
        }

        private CancellationToken RefreshToken()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }
}