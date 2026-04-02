using System;
using System.Collections.Generic;
using _Scripts._Shared.Sound;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.Data
{
    public class SfxListData<T> : ScriptableObject, IInitializable where T : Enum
    {
        [SF] protected List<SfxInfo<T>> sfxList;
        
        private readonly Dictionary<T, SfxInfo<T>> _sfxDict = new();
        private SoundManager _soundManager;

        public void Inject(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        // RegisterInstance는 주입을 안 하는 가봐
        // RegisterComponent는 주입을 해주는데
        
        // [Inject] 
        // protected void Construct(SoundManager soundManager)
        // {
        //     _soundManager = soundManager;
        //     Debug.Log($"<color=green>[SFX]</color> {this.GetType().Name} Construct : manager? {soundManager != null}");
        // }

        public void Initialize()
        {
            _sfxDict.Clear();

            foreach (var sfx in sfxList)
            {
                _sfxDict.Add(sfx.Type, sfx);
            }
            Debug.Log($"<color=green>[SFX]</color> {this.GetType().Name} Initialize");
        }

        private SfxInfo<T> GetInfo(T sfxType)
        {
            return _sfxDict.GetValueOrDefault(sfxType);
        }

        public SfxBuilder Play(T sfxType)
        {
            var info = GetInfo(sfxType);
            Debug.Log($"<color=green>[SFX]</color> {this.GetType().Name} Play : info? {info.IsValid} / manager? {_soundManager != null}");
            return info.IsValid ? _soundManager?.PlaySfx(info) : null;
        }
    }
}