using System;
using UnityEngine;
using UnityEngine.Audio;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.Data
{
    [Serializable]
    public struct SfxInfo<T>
    {
        [SF] private T type;
        [SF] private AudioClip clip;
        [SF] private AudioMixerGroup mixerGroup;
        [SF] private bool isLoop;

        public bool IsValid => clip != null;
        
        public T Type => type;
        public AudioClip Clip => clip;
        public AudioMixerGroup MixerGroup => mixerGroup;
        public bool IsLoop => isLoop;
    }
}