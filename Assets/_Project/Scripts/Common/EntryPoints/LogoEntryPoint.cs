using System.Collections.Generic;
using _Project.Scripts.Effects;
using _Project.Scripts.HUD;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Common.EntryPoints
{
    public class LogoEntryPoint : MonoBehaviour
    {
        public static StartOptions StartOptions { get; private set; }
        public static LogoEntryPoint Instance { get; private set; }
        
        [SerializeField] private StartOptions startOptions;
        
        public TextMeshProUGUI xClick; // Optional legacy label; safe to remove from the scene.
        [SerializeField] private TMP_FontAsset holdHintFont;
        [SerializeField, Min(0.2f)] private float startHoldDuration = 1.4f;
        private float _startHold;
        private HoldStartIndicator _holdIndicator;
        private GameObject _holdPrompt;
        private CanvasGroup _hintGroup;
        private TextMeshProUGUI _hintLabel;
        private float _hintAge;
        private bool _startingGame;
        private bool _gameStarted;
        private GameObject[] _presentationRoots;
        private Camera _menuCamera;
        private CanvasGroup _transitionCover;
        private PhysicsScene _menuPhysics;
        private SphereController[] _menuSpheres;
        private float _menuPhysicsTime;
        private bool _usesLocalMenuPhysics;
        public bool IsStartingGame => _startingGame && !_gameStarted;
        private readonly List<Canvas> _hiddenGameCanvases = new();
        private readonly List<UnityEngine.Rendering.Volume> _hiddenGameVolumes = new();

        private void Update()
        {
            if (!_startingGame && _holdIndicator != null)
            {
                var available = startOptions.TitleIntroFinished && !BootstrapEntryPoint.IsPaused && Application.isFocused;
                _holdPrompt.SetActive(available);
                if (available)
                {
                    _hintAge += Time.unscaledDeltaTime;
                    _hintGroup.alpha = Mathf.SmoothStep(0f,1f,Mathf.Clamp01(_hintAge / 0.4f));
                    _hintLabel.maxVisibleCharacters = Mathf.Max(0,Mathf.FloorToInt((_hintAge - 0.35f) * 35f));
                }
                var held = available && (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space));
                _startHold = held ? Mathf.Min(_startHold + Time.unscaledDeltaTime, startHoldDuration) : 0f;
                _holdIndicator.SetProgress(_startHold / startHoldDuration);
                var entrance = Mathf.SmoothStep(0f,1f,Mathf.Clamp01(_hintAge / 0.45f));
                var targetScale = Mathf.Lerp(0.92f,1f,entrance) + (held ? 0.025f : 0f);
                _holdIndicator.transform.localScale = Vector3.Lerp(_holdIndicator.transform.localScale,
                    Vector3.one * targetScale, 1f-Mathf.Exp(-Time.unscaledDeltaTime*14f));
                if (_startHold >= startHoldDuration) StartGame().Forget();
            }
            UpdateMenuPhysics();
        }

        private void UpdateMenuPhysics()
        {
            if (!_usesLocalMenuPhysics) return;
            if (_menuCamera == null || !_menuCamera.gameObject.activeInHierarchy)
            {
                _menuPhysicsTime = 0;
                return;
            }
            _menuPhysicsTime += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            const float step = 0.02f;
            while (_menuPhysicsTime >= step)
            {
                foreach (var sphere in _menuSpheres)
                    if (sphere != null && sphere.isActiveAndEnabled) sphere.StepMenuPhysics(step);
                _menuPhysics.Simulate(step);
                _menuPhysicsTime -= step;
            }
        }

        private void CreateHoldPrompt()
        {
            _holdPrompt = new GameObject("Hold to enter", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(CanvasGroup));
            _holdPrompt.transform.SetParent(transform, false);
            _hintGroup = _holdPrompt.GetComponent<CanvasGroup>();
            _hintGroup.alpha = 0f; _hintGroup.blocksRaycasts = false;
            var canvas = _holdPrompt.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 800;
            var scaler = _holdPrompt.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
            var circle = new GameObject("Hold ring", typeof(RectTransform), typeof(HoldStartIndicator));
            circle.transform.SetParent(_holdPrompt.transform,false);
            _holdIndicator = circle.GetComponent<HoldStartIndicator>(); _holdIndicator.raycastTarget=false;
            var rect = circle.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=new Vector2(0,0.5f);
            rect.anchoredPosition=new Vector2(115,0); rect.sizeDelta=new Vector2(84,84);
            var text = new GameObject("Hold hint",typeof(RectTransform),typeof(TextMeshProUGUI));
            text.transform.SetParent(_holdPrompt.transform,false);
            var label=text.GetComponent<TextMeshProUGUI>(); _hintLabel=label; label.maxVisibleCharacters=0; label.font=holdHintFont != null ? holdHintFont : TMP_Settings.defaultFontAsset; label.fontSize=21;
            label.color=new Color(0.78f,0.82f,0.73f,0.85f); label.raycastTarget=false;
            label.text="HOLD TO ENTER\n<size=14>LMB  /  SPACE</size>";
            var tr=label.rectTransform; tr.anchorMin=tr.anchorMax=new Vector2(0,0.5f); tr.pivot=new Vector2(0,0.5f);
            tr.anchoredPosition=new Vector2(174,0); tr.sizeDelta=new Vector2(270,80);
        }

        private void Start()
        {
            Instance = this;
            _presentationRoots = gameObject.scene.GetRootGameObjects();
            _menuCamera = Camera.main;
            _menuPhysics = gameObject.scene.GetPhysicsScene();
            _usesLocalMenuPhysics = _menuPhysics.IsValid() && _menuPhysics != Physics.defaultPhysicsScene;
            var spheres = new List<SphereController>();
            foreach (var root in _presentationRoots) spheres.AddRange(root.GetComponentsInChildren<SphereController>(true));
            _menuSpheres = spheres.ToArray();
            if (_usesLocalMenuPhysics)
                foreach (var sphere in _menuSpheres) sphere.UseMenuPhysics(_menuCamera);
            CreateTransitionCover();
            SceneManager.sceneLoaded += OnSceneLoaded;
            Cursor.visible = false;
            StartOptions = startOptions;
            startOptions.SetVisible(false);

            if (xClick != null) xClick.gameObject.SetActive(false);
            CreateHoldPrompt();
            _holdPrompt.SetActive(false);
        }

        private async UniTask StartGame()
        {
            if (_startingGame) return;
            _startingGame = true;
            _holdPrompt.SetActive(false);
            if (xClick != null) xClick.gameObject.SetActive(false);
            Cursor.visible = false;
            await startOptions.PlayTitleExit();
            if (this == null) return;
            await FadeCover(1, 0.18f);
            if (this == null) return;

            // Loading is hidden: no gameplay camera or volume exists during the visible title exit.
            PrepareMenuRendering();
            // Render the real title camera until the loaded game has a camera ready.
            _menuCamera.tag = "Untagged";
            _menuCamera.depth = 100;
            var loading = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
            await loading;
            if (this == null) return;
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(2));
            await UniTask.WaitUntil(() => Camera.main != null && Camera.main.gameObject.scene.buildIndex == 2);
            if (this == null) return;
            SetPresentationVisible(false);
            startOptions.RestoreTitleAfterExit();
            OutlineMover.Initialize(0.11f);
            await FadeCover(0, 0.3f);
            if (this == null) return;
            _gameStarted = true;
        }

        private void CreateTransitionCover()
        {
            var cover = new GameObject("MenuTransition", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.GraphicRaycaster), typeof(CanvasGroup), typeof(UnityEngine.UI.Image));
            cover.transform.SetParent(transform, false);
            var canvas = cover.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingLayerID = startOptions.GetComponent<Canvas>().sortingLayerID;
            canvas.sortingOrder = 950;
            cover.GetComponent<UnityEngine.UI.Image>().color = Color.black;
            _transitionCover = cover.GetComponent<CanvasGroup>();
            _transitionCover.alpha = 0;
            _transitionCover.blocksRaycasts = false;
        }

        private async UniTask FadeCover(float alpha, float duration)
        {
            _transitionCover.blocksRaycasts = true;
            await _transitionCover.DOFade(alpha, duration).SetEase(Ease.InOutSine)
                .SetUpdate(true).SetLink(gameObject).AsyncWaitForCompletion();
            if (this != null) _transitionCover.blocksRaycasts = alpha > 0;
        }

        public async UniTask TransitionPause(bool visible)
        {
            if (!_gameStarted)
            {
                if (visible) await startOptions.OpenPause();
                else await startOptions.FadeVisible(false, 0.25f);
                return;
            }
            if (!visible) startOptions.PrepareToClose();
            await FadeCover(1, 0.18f);
            if (this == null) return;
            SetPauseBackground(visible);
            startOptions.SetVisible(false);
            await FadeCover(0, 0.25f);
            if (this == null) return;
            // Let the departure run on the visible menu, rather than behind the camera-switch cover.
            if (visible) await startOptions.OpenPause();
        }

        private void PrepareMenuRendering()
        {
            var layer = LayerMask.NameToLayer("MenuPresentation");
            foreach (var root in _presentationRoots)
                if (root != gameObject)
                    foreach (var child in root.GetComponentsInChildren<Transform>(true))
                        child.gameObject.layer = layer;
            _menuCamera.cullingMask = 1 << layer;
            _menuCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().volumeLayerMask = 1 << layer;
            var listener = _menuCamera.GetComponent<FMODUnity.StudioListener>();
            if (listener != null) listener.enabled = false;
        }

        public void SetPauseBackground(bool visible)
        {
            if (!_gameStarted) return;
            if (visible)
            {
                HideGameplayPresentation();
                SetPresentationVisible(true);
            }
            else
            {
                SetPresentationVisible(false);
                foreach (var canvas in _hiddenGameCanvases)
                    if (canvas != null) canvas.enabled = true;
                foreach (var volume in _hiddenGameVolumes)
                    if (volume != null) volume.enabled = true;
                _hiddenGameCanvases.Clear();
                _hiddenGameVolumes.Clear();
            }
        }

        private void SetPresentationVisible(bool visible)
        {
            if (_menuCamera != null)
            {
                // The gameplay MainCamera remains available to ongoing encounter initialization.
                _menuCamera.tag = visible && !_startingGame ? "MainCamera" : "Untagged";
                if (_startingGame) _menuCamera.depth = 100;
            }
            foreach (var root in _presentationRoots)
                if (root != gameObject && root != null) root.SetActive(visible);
        }

        private void HideGameplayPresentation()
        {
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (canvas.enabled && canvas.gameObject.scene != gameObject.scene &&
                    (canvas.gameObject.scene.buildIndex == 2 || canvas.gameObject.scene.buildIndex == 3))
                {
                    _hiddenGameCanvases.Add(canvas);
                    canvas.enabled = false;
                }
            foreach (var volume in FindObjectsByType<UnityEngine.Rendering.Volume>(FindObjectsSortMode.None))
                if (volume.enabled && volume.gameObject.scene != gameObject.scene &&
                    (volume.gameObject.scene.buildIndex == 2 || volume.gameObject.scene.buildIndex == 3))
                {
                    _hiddenGameVolumes.Add(volume);
                    volume.enabled = false;
                }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_gameStarted && BootstrapEntryPoint.IsPaused) HideGameplayPresentation();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this)
            {
                Instance = null;
                StartOptions = null;
            }
        }
    }
}
