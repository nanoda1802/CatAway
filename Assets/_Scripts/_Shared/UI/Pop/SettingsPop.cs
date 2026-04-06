using _Scripts._Helper;
using _Scripts._Shared.Data;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.UI.Pop
{
    public class SettingsPop : PopBase
    {
        [Header("[ Components ]")]
        [SF] private RectTransform rectTr;
        [SF] private Slider bgmSlider;
        [SF] private Slider sfxSlider;
        [SF] private Toggle bgmToggle;
        [SF] private Toggle sfxToggle;
        
        [Header("[ Tween Settings ]")]
        [SF] private TweenSettings<float> popUpPosSettings;
        [SF] private TweenSettings<float> popUpAlphaSettings;
        [SF] private TweenSettings<float> popDownPosSettings;
        [SF] private TweenSettings<float> popDownAlphaSettings;
        
        private TweenHandler _tweenHandler;
        private SoundSettingsData _soundSettings;

        [Inject]
        private void Construct(
            TweenHandler tweenHandler,
            SoundSettingsData soundSettings)
        {
            _tweenHandler = tweenHandler;
            _soundSettings = soundSettings;
        }

        protected override void PopUp()
        {
            SyncValues();
            base.PopUp();
            CurSequence = _tweenHandler.AnchorPosY(ViewGroup, rectTr, popUpAlphaSettings, popUpPosSettings, OnPopUpCompleted);
        }

        protected override void PopDown()
        {
            if (CurSequence.isAlive) CurSequence.Complete();
            CurSequence = _tweenHandler.AnchorPosY(ViewGroup, rectTr, popDownAlphaSettings, popDownPosSettings, OnPopDownCompleted);
            
            Bg.OnClick -= PopDown;
            RemoveHandlers();
        }
        
        private void OnPopUpCompleted()
        {
            Bg.OnClick += PopDown;
            AddHandlers();
        }

        private void OnPopDownCompleted()
        {
            base.PopDown();
        }

        private void SyncValues()
        {
            bgmSlider.value = _soundSettings.BgmVolume;
            sfxSlider.value = _soundSettings.SfxVolume;
            bgmToggle.isOn = _soundSettings.IsBgmMute;
            sfxToggle.isOn = _soundSettings.IsSfxMute;
        }

        private void AddHandlers()
        {
            RemoveHandlers();
            
            bgmSlider.onValueChanged.AddListener(v => _soundSettings.BgmVolume = v);
            sfxSlider.onValueChanged.AddListener(v => _soundSettings.SfxVolume = v);
            bgmToggle.onValueChanged.AddListener(v => _soundSettings.IsBgmMute = v);
            sfxToggle.onValueChanged.AddListener(v => _soundSettings.IsSfxMute = v);
        }

        private void RemoveHandlers()
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.RemoveAllListeners();
            bgmToggle.onValueChanged.RemoveAllListeners();
            sfxToggle.onValueChanged.RemoveAllListeners();
        }
    }
}