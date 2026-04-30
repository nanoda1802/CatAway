using UnityEngine;

namespace _Scripts._Helper
{
    public class VfxHandler
    {
        public void PlayVfx(ParticleSystem vfx)
        {
            if (vfx is null) return;
            if (vfx.isPlaying) StopImmediately(vfx);
            
            vfx.Play();
        }

        public void StopSmoothly(ParticleSystem vfx) // StopEmitting : 추가 파티클만 막음, 이미 나온 녀석들은 남아서 마저 진행됨
        {
            vfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        public void StopImmediately(ParticleSystem vfx) // StopEmittingAndClear : 아예 모든 파티클 제거
        {
            vfx?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}