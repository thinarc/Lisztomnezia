using _Project.Scripts.Effects;
using _Project.Scripts.Managers;
using _Project.Scripts.osu_;
using Cysharp.Threading.Tasks;
using SmnStyleHardline.Core;
using TMPEffects.Components;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Common.EntryPoints
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        public static BootstrapEntryPoint Instance { get; private set; }

        private void Awake() => SetEscapeText("");
        
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            
            LangManager.InitLocale(0);
            
            PlayerPrefs.SetFloat("bg", 1f);
            PlayerPrefs.SetFloat("sfx", 0.75f);
            PlayerPrefs.SetFloat("voice", 0.6f);
            SoundManager.Instance.Initialize();
            
            Instance = this;
            DontDestroyOnLoad(this);

            OsuContainer.Initialized = false;
            
            OutlineMover.Initialize(3f);
            SceneManager.LoadScene(1, new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.Physics3D));
            
            // OutlineMover.Initialize(0.11f);
            // SceneManager.LoadScene(2);
        }

        public static bool IsPaused => Instance != null && Instance._paused;

        public TextMeshProUGUI escText;
        private bool _paused;
        private bool _closeOnRelease;
        private bool _quitting;
        private float _heldEscape;
        private float _previousTimeScale;
        private bool _previousCursorVisible;
        private CursorLockMode _previousCursorLock;
        private readonly string[] _busPaths = { "bus:/bg", "bus:/osu", "bus:/sfx", "bus:/voice" };
        private readonly FMOD.Studio.Bus[] _pausedBuses = new FMOD.Studio.Bus[4];
        private readonly bool[] _previousBusPaused = new bool[4];
        private bool _previousAudioPaused;
        private bool _pauseTransitionRunning;
        private bool _requestedPaused;

        private void Update()
        {
            HandleEscape(Input.GetKeyDown(KeyCode.Escape), Input.GetKey(KeyCode.Escape),
                Input.GetKeyUp(KeyCode.Escape), Time.unscaledDeltaTime);
        }

        private void HandleEscape(bool pressed, bool held, bool released, float deltaTime)
        {
            if (_quitting) return;
            if (LogoEntryPoint.Instance != null && LogoEntryPoint.Instance.IsStartingGame)
            {
                SetEscapeText("");
                return;
            }
            var options = LogoEntryPoint.StartOptions;
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            var canPause = sceneIndex == 1 || sceneIndex == 2;

            if (pressed)
            {
                _closeOnRelease = _paused;
                _heldEscape = 0;
                if (!_paused && canPause && options != null) SetPaused(true);
            }

            if (held && _paused)
            {
                _heldEscape += deltaTime;
                SetEscapeText(_heldEscape < 0.35f ? "" : _heldEscape < 0.9f ? "<<<" : _heldEscape < 2f ? "<<" : "<");
                if (_heldEscape >= 2f) QuitGame().Forget();
            }
            else SetEscapeText("");

            if (released)
            {
                // The opening tap leaves the menu open; releasing the next press resumes.
                if (_closeOnRelease) SetPaused(false);
                _closeOnRelease = false;
                _heldEscape = 0;
                SetEscapeText("");
            }
        }

        private void SetEscapeText(string text)
        {
            if (escText == null) return;
            var writer = escText.GetComponent<TMPWriter>();
            if (writer != null) writer.enabled = false;
            escText.text = text;
            escText.enabled = text.Length > 0;
        }

        public void SetPaused(bool paused)
        {
            _requestedPaused = paused;
            if (!_pauseTransitionRunning) TransitionPause().Forget();
        }

        private async UniTask TransitionPause()
        {
            _pauseTransitionRunning = true;
            try
            {
                // Serialize transitions; rapid Escape presses update the requested final state.
                while (_requestedPaused != _paused)
                {
                    var opening = _requestedPaused;
                    if (opening) ApplyGameplayPause(true);
                    if (LogoEntryPoint.Instance != null)
                        await LogoEntryPoint.Instance.TransitionPause(opening);
                    else if (LogoEntryPoint.StartOptions != null)
                        LogoEntryPoint.StartOptions.SetVisible(opening);
                    if (this == null || !isActiveAndEnabled) return;
                    if (!opening) ApplyGameplayPause(false);
                }
            }
            finally
            {
                _pauseTransitionRunning = false;
            }
        }

        private void ApplyGameplayPause(bool paused)
        {
            if (_paused == paused) return;
            if (paused)
            {
                _previousTimeScale = Time.timeScale;
                _previousCursorVisible = Cursor.visible;
                _previousCursorLock = Cursor.lockState;
                _previousAudioPaused = AudioListener.pause;
                // Leave the master bus to FMOD's application-focus pause handling.
                for (var i = 0; i < _busPaths.Length; i++)
                {
                    _pausedBuses[i] = FMODUnity.RuntimeManager.GetBus(_busPaths[i]);
                    _pausedBuses[i].getPaused(out _previousBusPaused[i]);
                    _pausedBuses[i].setPaused(true);
                }
                _paused = true;
                Time.timeScale = 0;
                AudioListener.pause = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                _paused = false;
                Time.timeScale = _previousTimeScale;
                AudioListener.pause = _previousAudioPaused;
                for (var i = 0; i < _pausedBuses.Length; i++)
                    if (_pausedBuses[i].isValid()) _pausedBuses[i].setPaused(_previousBusPaused[i]);
                Cursor.lockState = _previousCursorLock;
                Cursor.visible = _previousCursorVisible;
            }
        }

        private async UniTask QuitGame()
        {
            _quitting = true;
            await UniTask.Delay(200, ignoreTimeScale: true);
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        private void OnDisable()
        {
            _requestedPaused = false;
            if (_paused) ApplyGameplayPause(false);
            OutlineMover.OnDisable();
        }
    }
}
