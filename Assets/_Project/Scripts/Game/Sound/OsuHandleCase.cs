using System;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;

namespace _Project.Scripts.Sound
{
    public class OsuHandleCase
    {
        private EventInstance _fail;
        private EventInstance _hit;
        private EventInstance _slide;
        private EventInstance _softHit;
        private EventInstance _startOsu;

        public void SFail()
        {
            _fail = RuntimeManager.CreateInstance("event:/Act_QTE/Fail");
            _fail.start();
        }
        
        public void SHit()
        {
            _hit = RuntimeManager.CreateInstance("event:/Act_QTE/Hit");
            _hit.start();
        }
        
        public void SSlide()
        {
            _slide = RuntimeManager.CreateInstance("event:/Act_QTE/Slide");
            _slide.start();
        }
        
        public void SSoftHit()
        {
            _softHit = RuntimeManager.CreateInstance("event:/Act_QTE/Soft Hit");
            _softHit.start();
        }
        
        public async UniTask SStartOsu()
        {
            _startOsu = RuntimeManager.CreateInstance("event:/Act_QTE/Start OSU");
            _startOsu.start();
            
            // while (true)
            // {
            //     _startOsu.getPlaybackState(out PLAYBACK_STATE state);
            //     if (state == PLAYBACK_STATE.STOPPING) break;
            //     await UniTask.Yield();
            // }
            
            await UniTask.Delay(TimeSpan.FromSeconds(1.8f));
        }
    }
}