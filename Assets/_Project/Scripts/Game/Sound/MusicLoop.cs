using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _Project.Scripts.Sound
{
    public class MusicLoop
    {
        private EventInstance _background;
        private EventInstance _additional3d;
        
        public void Initialize()
        {
            _background = RuntimeManager.CreateInstance("event:/Music/EV_Amb");
            _background.start();
            // _background.setPaused(true);
        }

        public void CallAdditional3d(GameObject go)
        {
            _additional3d = RuntimeManager.CreateInstance("event:/Music/Noise");
            _additional3d.set3DAttributes(go.To3DAttributes());
            _additional3d.start();
            // _additional3d.setPaused(true);
        }

        public void KillAdditional3d(GameObject go)
        {
            _additional3d.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
}