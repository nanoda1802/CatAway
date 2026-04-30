using System;
using System.Collections.Generic;
using _Scripts.Shared.Sound;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Shared._Data
{
    public class SfxListData<T> : ScriptableObject, IInitializable where T : Enum
    {
        [SF] protected List<SfxInfo<T>> sfxList;
        
        private readonly Dictionary<T, SfxInfo<T>> _sfxDict = new();
        private SoundManager _soundManager;

        // public void Inject(SoundManager soundManager)
        // {
        //     _soundManager = soundManager;
        // }

        [Inject]
        private void Construct(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        public void Initialize()
        {
            _sfxDict.Clear();

            foreach (var sfx in sfxList)
            {
                _sfxDict.Add(sfx.Type, sfx);
            }
        }

        private SfxInfo<T> GetInfo(T sfxType)
        {
            return _sfxDict.GetValueOrDefault(sfxType);
        }

        public SfxBuilder Play(T sfxType, bool withRandomPitch = false, Transform tr = null)
        {
            var info = GetInfo(sfxType);
            return info.IsValid ? _soundManager?.PlaySfx(info, withRandomPitch, tr) : null;
        }
    }
}