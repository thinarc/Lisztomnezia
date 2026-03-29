using FMOD.Studio;
using FMODUnity;

namespace _Project.Scripts.Sound
{
    public class MusicLoop
    {
        private EventInstance _amb;
        
        public void Initialize()
        {
            _amb = RuntimeManager.CreateInstance("event:/Music/Amb+Noise");
            _amb.start();
            _amb.setParameterByName("Amb_Int", 2);
        }

        public void SetPauseAmb(bool pause)
        {
            _amb.setPaused(pause);
        }

        public void CallNoise()
        {
            _amb.setParameterByName("Amb_Int", 0);
        }

        public void KillNoise()
        {
            _amb.setParameterByName("Amb_Int", 2);
        }
    }
}