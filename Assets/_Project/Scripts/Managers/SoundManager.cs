using _Project.Scripts.Sound;
using FMOD.Studio;
using UnityEngine;

namespace _Project.Scripts.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        public MusicLoop MusicLoop { get; private set; }
        public OsuCase OsuCase { get; private set; }
        public OsuHandleCase OsuHandleCase { get; private set; }
        public PoolPhrases PoolPhrases { get; private set; }

        private Bus _bgBus;
        private Bus _osuBus;
        private Bus _sfxBus;
        private Bus _voiceBus;
        
        private float _bgVolume;

        public float BgVolume
        {
            get => _bgVolume;
            set
            {
                if (value is > 1f or < 0) return;
                _bgVolume = value;
                _bgBus.setVolume(_bgVolume);
                _osuBus.setVolume(_bgVolume);
                PlayerPrefs.SetFloat("bg", _bgVolume);
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
                _sfxBus.setVolume(_sfxVolume);
                PlayerPrefs.SetFloat("sfx", _sfxVolume);
            }
        }
        
        private float _voiceVolume;
        
        public float VoiceVolume
        {
            get => _voiceVolume;
            set
            {
                if (value is > 1f or < 0) return;
                _voiceVolume = value;
                _voiceBus.setVolume(_voiceVolume);
                PlayerPrefs.SetFloat("voice", _voiceVolume);
            }
        }

        private void Update()
        {
            if (_Project.Scripts.Common.EntryPoints.BootstrapEntryPoint.IsPaused) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.K))
            {
                Time.timeScale = 10;
                _osuBus.getChannelGroup(out var ch);
                _osuBus.setVolume(0);
                if (ch.hasHandle()) ch.setPitch(Time.timeScale * 2);
            }
            else if (Input.GetKeyUp(KeyCode.K))
            {
                Time.timeScale = 1;
                _osuBus.getChannelGroup(out var ch);
                _osuBus.setVolume(_bgVolume);
                if (ch.hasHandle()) ch.setPitch(Time.timeScale);
            }
#endif
        }

        private void Awake() => Instance = this;

        public void Initialize()
        {
            _bgBus = FMODUnity.RuntimeManager.GetBus("bus:/bg");
            _osuBus = FMODUnity.RuntimeManager.GetBus("bus:/osu");
            _sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/sfx");
            _voiceBus = FMODUnity.RuntimeManager.GetBus("bus:/voice");
            
            BgVolume = PlayerPrefs.GetFloat("bg", 1f);
            SfxVolume = PlayerPrefs.GetFloat("sfx", 0.75f);
            VoiceVolume = PlayerPrefs.GetFloat("voice", 0.6f);

            MusicLoop = new MusicLoop();
            MusicLoop.Initialize();
            
            OsuCase = new OsuCase();
            OsuCase.Initialize();
            
            OsuHandleCase = new OsuHandleCase();
        }
        
        public void InjectPhrases(PoolPhrases inst) => PoolPhrases = inst;
    }
}