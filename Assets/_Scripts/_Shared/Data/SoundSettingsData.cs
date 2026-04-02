using System;
using _Scripts._Shared.Sound;
using _Scripts.Scene_Stage.Data.Level;
using UnityEngine;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.Data
{
    [CreateAssetMenu(fileName = "Settings", menuName = "SO/Settings")]
    public class SoundSettingsData : ScriptableObject, IInitializable, IDisposable
    {
        [SF] private ProviderInfo<SfxBuilder> providerInfo;
        [SF] private float bgmFadeSpeed = 1.5f;

        [Header("[ Volume ]")]
        [SF] private float bgmVolume;
        [SF] private float sfxVolume;
        [Header("[ Mute ]")]
        [SF] private bool isSfxMute;
        [SF] private bool isBgmMute;
        
        public ProviderInfo<SfxBuilder> ProviderInfo => providerInfo;
        public float VolumeFadeDelta => bgmVolume * bgmFadeSpeed * Time.unscaledDeltaTime;
        
        public event Action<float> BgmVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        
        public event Action<bool> BgmMuteToggled;
        public event Action<bool> SfxMuteToggled;

        public float BgmVolume
        {
            get => bgmVolume;
            set
            {
                bgmVolume = value;
                BgmVolumeChanged?.Invoke(value);
            }
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = value;
                SfxVolumeChanged?.Invoke(value);
            }
        }

        public bool IsBgmMute
        {
            get => isBgmMute;
            set
            {
                isBgmMute = value;
                BgmMuteToggled?.Invoke(value);
            }
        }
        
        public bool IsSfxMute
        {
            get => isSfxMute;
            set
            {
                isSfxMute = value;
                SfxMuteToggled?.Invoke(value);
            }
        }

        public void Initialize()
        {
            // playerPrefs 에서 저장한 string 가져옴
            // json으로 변환
            // 필드별로 저장된 값 할당
        }

        public void Dispose()
        {
            // 현 필드와 값들 json으로 추출
            // string으로 변환
            // playerPrefs에 저장
        }
    }
}