using FMOD.Studio;
using FMODUnity;

namespace _Project.Scripts.Sound
{
    public class OsuCase
    {
        private EventInstance _case;

        public void Initialize() { }

        public void StartLevel()
        {
            _case = RuntimeManager.CreateInstance("event:/QTE Sound/QTE");
            _case.start();
        }
    }
}