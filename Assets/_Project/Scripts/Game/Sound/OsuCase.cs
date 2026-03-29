using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

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
            _case.setParameterByName("QTE_Star", 1f);
        }

        public float[] GetDesc()
        {
            _case.getTimelinePosition(out var pos);
            _case.getDescription(out var desc);
            desc.getLength(out var length);

            var normalized = (float)pos / length;
            
            return new [] { pos, length, normalized };
        }

        public void RepLevel()
        {
            _case.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _case = RuntimeManager.CreateInstance("event:/QTE Sound/QTE");
            _case.start();
            _case.setParameterByName("QTE_Star", 1f);
        }

        public bool CaseChange(float val)
        {
            _case.getParameterByName("QTE_Star", out var current);
            if (current >= 1f) return true;
            _case.setParameterByName("QTE_Star", current + val);
            return false;
        }

        public void StopLevel()
        {
            _case.setParameterByName("QTE_Star", Random.Range(0.8f, 0.95f));
        }
    }
}