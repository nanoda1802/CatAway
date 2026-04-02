using System;
using System.Collections.Generic;
using _Scripts._Shared.Data;
using _Scripts.Scene_Stage;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.Sound
{
    public class SoundManager : MonoBehaviour
    {
        private SoundSettingsData _soundSettings;
        private SfxProvider _sfxProvider;
        
        private AudioSource _bgmAudioSource;
        
        private readonly List<SfxBuilder> _activeSfx = new();

        private float BgmVolume
        {
            get => _bgmAudioSource.volume;
            set => _bgmAudioSource.volume = value;
        }
        
        private AudioClip CurBgmClip
        {
            get => _bgmAudioSource.clip;
            set => _bgmAudioSource.clip = value;
        }
        
        [Inject]
        private void Construct(
                SoundSettingsData soundSettings,
                SfxProvider sfxProvider
            )
        {
            _bgmAudioSource = GetComponent<AudioSource>();
            
            _soundSettings = soundSettings;
            _sfxProvider = sfxProvider;

            _soundSettings.BgmVolumeChanged += OnBgmVolumeChanged;
            _soundSettings.SfxVolumeChanged += OnSfxVolumeChanged;
            _soundSettings.BgmMuteToggled += OnBgmMuteToggled;
            _soundSettings.SfxMuteToggled += OnSfxMuteToggled;
        }

        public void TogglePause(bool isPaused)
        {
            foreach (var sfx in _activeSfx)     
            {
                sfx.TogglePause(isPaused);
            }
            
            if (isPaused) _bgmAudioSource.Pause();
            else _bgmAudioSource.UnPause();
        }
        
        #region Settings Event 관련 메서드
        private void OnBgmVolumeChanged(float volume)
        {
            BgmVolume = volume;
        }

        private void OnBgmMuteToggled(bool isMute)
        {
            _bgmAudioSource.mute = isMute;
        }
    
        private void OnSfxVolumeChanged(float volume)
        {
            foreach (var sfx in _activeSfx)     
            {
                sfx.Volume = volume;
            }
        }

        private void OnSfxMuteToggled(bool isMute)
        {
            foreach (var sfx in _activeSfx)     
            {
                sfx.IsMute = isMute;
            }
        }
        #endregion

        #region Bgm 관련 메서드
        public async UniTaskVoid StopBgm(bool immediately = false)
        {
            if (!immediately && CurBgmClip != null) await FadeOutBgm();
            
            BgmVolume = 0;
            
            _bgmAudioSource.Stop();
            CurBgmClip = null;
        }
        
        public async UniTaskVoid PlayBgm(AudioClip clip)
        {
            if (clip is null) return;
            if (CurBgmClip != null) StopBgm(true).Forget();
            
            CurBgmClip = clip;
            _bgmAudioSource.Play();
    
            await FadeInBgm();
            
            BgmVolume = _soundSettings.BgmVolume;
        }
    
        private async UniTask FadeOutBgm()
        {
            BgmVolume = _soundSettings.BgmVolume;
            
            while (BgmVolume > 0)
            {
                BgmVolume -= _soundSettings.VolumeFadeDelta;
                await UniTask.Yield(cancellationToken:this.destroyCancellationToken);
            }
        }
    
        private async UniTask FadeInBgm()
        {
            BgmVolume = 0;
            
            while (BgmVolume < _soundSettings.BgmVolume)
            {
                BgmVolume += _soundSettings.VolumeFadeDelta;
                await UniTask.Yield(cancellationToken:this.destroyCancellationToken);
            }   
        } 
        #endregion

        #region Sfx 관련 메서드
        public void StopAllSfx()
        {
            foreach (var sfx in _activeSfx)
            {
                if (sfx == null) continue;
             
                sfx.Stop();
            }
            
            _activeSfx.Clear();
        }

        public SfxBuilder PlaySfx<T>(SfxInfo<T> info, bool randomPitch = false, Transform sourceTr = null) where T : Enum
        {
            var sfx = _sfxProvider.GetEmitter()?
                .WithInfo(info.Clip, info.IsLoop)
                .WithRandomPitch(randomPitch)
                .WithPos(sourceTr);
            
            Debug.Log($"<color=green>[SFX]</color> {this.GetType().Name} PlaySfx : info? {info.IsValid} / clip? {info.Clip?.name} / loop? {info.IsLoop} / sfx? {sfx !=null}");
            
            bool hasPlay = sfx != null && sfx.Play();
            
            if (hasPlay) _activeSfx.Add(sfx);

            return sfx;
        }

        public void StopSfx(SfxBuilder sfx)
        {
            sfx.Stop();
        }

        public void ReleaseBuilder(SfxBuilder builder)
        {
            _activeSfx.Remove(builder);
            _sfxProvider.Release(builder);
        }
        #endregion
    }
}