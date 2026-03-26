using _Project.Scripts.Sound;
using UnityEngine;

namespace _Project.Scripts.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        public MusicLoop MusicLoop { get; private set; }
        
        private float _bgVolume;

        public float BgVolume
        {
            get => _bgVolume;
            set
            {
                if (value is > 1f or < 0) return;
                _bgVolume = value;
            }
        }
        
        private float _sfxVolume;
        
        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                if (value is > 1f or < 0) return;
                _sfxVolume = value;
            }
        }

        public void Start()
        {
            Instance = this;
            
            _bgVolume = 1f;
            _sfxVolume = 0.75f;

            MusicLoop = new MusicLoop();
            MusicLoop.Initialize();
        }
    }
}