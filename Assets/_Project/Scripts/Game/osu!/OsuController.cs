using System;
using _Project.Scripts.Managers;
using _Project.Scripts.osu_.Align;
using _Project.Scripts.osu_.Hit;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EvolveGames;
using Sirenix.OdinInspector;
using SmnStyleHardline.Core;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Project.Scripts.osu_
{
    public class OsuController : MonoBehaviour
    {
        public static OsuController Instance { get; private set; }

        [SerializeField, Space(5)] private GameObject gameScreen;
        [SerializeField] private RawImage transition;
        private CanvasGroup _canvas;
        
        [SerializeField, Space(5)] private PlayerController player;
        [SerializeField] private GameObject cursorTrail;

        [SerializeField, Space(5)] private GameObject[] bassPrefabs;
        [SerializeField] private GameObject[] snarePrefabs;
        [SerializeField, Space(5)] private HitPositioner positioner;

        [Header("Difficult")] [SerializeField] private bool disableImport;
        [SerializeField] private GameObject import;
        
        [Header("General")]
        [SerializeField, Space(5)] private float hitDelay = 1f;
        
        [Header("Bass")]
        [SerializeField] private float bassTreshold = 0.7f;
        [SerializeField] private float bassDamping = 5f;
        [SerializeField] private bool disableBass;
        
        [Header("Snare")]
        [SerializeField] private float snareTreshold = 0.7f;
        [SerializeField] private float snareTailTreshold = 0.8f;
        [SerializeField] private float snareDamping = 5f;
        [SerializeField] private bool disableSnare;
        
        private float _bassValue;
        private bool _bassUsed;
        
        private float _snareValue;
        private bool _snareUsed;

        private float _hitDelay;
        private int _hitCounts;
        private int _hitLimit;
        private bool _run;

        [Header("User"), SerializeField] private Slider slider;
        [SerializeField] private Slider lenghtSlider;
        [SerializeField] private Slider speedSlider;

        private float _slider;
        private bool _fill;
        private bool _fillCalled;

        private void Start()
        {
            Instance = this;
            
            transition.material.SetFloat("_FadingFade", 0);
            transition.material.SetFloat("_GaussianBlurFade", 0);
            
            _canvas = GetComponent<CanvasGroup>();
            _canvas.DOFade(0, 0);
            positioner.Initialize();
            
            if (disableImport) import.SetActive(!disableImport);
            
            _hitLimit = 9;
            _hitCounts = 0;

            _slider = 0;
            slider.value = _slider;
            _fill = false;
            
            lenghtSlider.value = 0;
            speedSlider.value = 0.5f;
        }
        
        public async void RunGame()
        {
            await StartTransition();
            _run = true;
            _fill = true;
        }
        
        public void OnBass()
        {
            if (!_run || disableBass) return;
            _bassValue = 1f; ;
        }

        public void OnSnare()
        {
            if (!_run || disableSnare) return;
            _snareValue = 1f;
        }

        [Button] public void KickSlider()
        {
            _slider -= 0.2f;
            _slider = Mathf.Clamp(_slider, 0.001f, 1f);
            slider.DOValue(_slider, 0.4f).SetEase(Ease.OutSine);
        }
        
        [Button] public void RegenSlider()
        {
            _slider += 0.05f;
            _slider = Mathf.Clamp(_slider, 0.001f, 1f);
            slider.DOValue(_slider, 0.4f).SetEase(Ease.OutSine);
        }
        
        private void Update()
        {
            if (_fill)
            {
                if (!_fillCalled)
                {
                    slider.value = 0.001f;
                    _slider = 1;
                    slider.DOValue(_slider, 2.6f).SetEase(Ease.OutSine).OnComplete(() => _fill = false);
                }
                _fillCalled = true;
            }
            
            _bassValue = Mathf.Lerp(_bassValue, 0f, bassDamping * Time.deltaTime);
            _snareValue = Mathf.Lerp(_snareValue, 0f, snareDamping * Time.deltaTime);

            if (_bassValue > bassTreshold && !_bassUsed && _hitDelay < 0)
            {
                _hitDelay = hitDelay;
                _hitDelay = hitDelay;
                _hitCounts++;
                
                var pos = positioner.GetNextPosition(false);
                var hit = Instantiate(bassPrefabs[0], transform);
                var rect = hit.GetComponent<RectTransform>();
                rect.anchoredPosition = pos;
                rect.SetAsFirstSibling();
                rect.name = $"bass_{_hitCounts}";
                
                var controller = hit.GetComponentInChildren<HitController>();
                controller.SetNumber(_hitCounts);

                if (_hitCounts >= _hitLimit) _hitCounts = 0;
            }
            
            if (_snareValue > snareTreshold && !_snareUsed && _hitDelay < 0)
            {
                _hitDelay = hitDelay;
                _hitDelay = hitDelay;
                _hitCounts++;
                
                var pos = positioner.GetNextPosition(true);
                var prefabIndex = 0;
                if (_snareValue > snareTailTreshold) prefabIndex = 1;
                var hit = Instantiate(snarePrefabs[prefabIndex], transform);
                var rect = hit.GetComponent<RectTransform>();
                rect.anchoredPosition = pos;
                if (prefabIndex == 1)
                {
                    rect.rotation = Quaternion.Euler(0, 0, 45);
                    rect.GetComponent<HitCompensateRot>().Compensate();
                }
                rect.SetAsFirstSibling();
                rect.name = $"snare_{_hitCounts}";
                
                var controller = hit.GetComponentInChildren<HitController>();
                controller.SetNumber(_hitCounts);
                
                if (_hitCounts >= _hitLimit) _hitCounts= 0;
            }

            if (_hitDelay < -0.9f) _hitCounts = 0;

            _bassUsed = _bassValue > bassTreshold;
            _snareUsed = _snareValue > snareTreshold;
            
            _hitDelay -= Time.deltaTime;
        }
        
        private async UniTask StartTransition()
        {
            player.enabled = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            
            gameScreen.SetActive(true);
            await UniTask.NextFrame();
            gameScreen.SetActive(false);
            
            _ = UniTask.WaitWhile(() =>
            {
                var val = Mathf.MoveTowards(transition.material.GetFloat("_GaussianBlurFade"), 1, Time.deltaTime * Random.Range(0.64f, 0.86f) * 1.2f);
                transition.material.SetFloat("_GaussianBlurFade", val);
                return !Mathf.Approximately(val, 1);
            });
            
            await UniTask.WaitWhile(() =>
            {
                var val = Mathf.MoveTowards(transition.material.GetFloat("_FadingFade"), 1, Time.deltaTime * Random.Range(0.8f, 0.96f) * 1.2f);
                transition.material.SetFloat("_FadingFade", val);
                return !Mathf.Approximately(val, 1);
            });
            
            GetComponent<Image>().material = transition.material;
            _canvas.DOFade(1, 0.4f).SetEase(Ease.InOutSine);
            await UniTask.Delay(TimeSpan.FromSeconds(0.08f));
            transition.DOFade(0, 0.1f).SetEase(Ease.OutQuad);
            
            SSHStyleManager.API.Outline.SetColorOutlineStrength(3f);
            
            SoundManager.Instance.MusicLoop.SetPauseAmb(true);
            SoundManager.Instance.OsuCase.StartLevel();
            
            await UniTask.WaitWhile(() =>
            {
                var val = Mathf.MoveTowards(transition.material.GetFloat("_FadingFade"), 0, Time.deltaTime * Random.Range(0.8f, 0.96f) * 2);
                transition.material.SetFloat("_FadingFade", val);
                return !Mathf.Approximately(val, 0);
            });
            
            _ = UniTask.WaitWhile(() =>
            {
                var val = Mathf.MoveTowards(transition.material.GetFloat("_GaussianBlurFade"), 0, Time.deltaTime * Random.Range(0.64f, 0.86f) * 2);
                transition.material.SetFloat("_GaussianBlurFade", val);
                return !Mathf.Approximately(val, 0);
            });
            
            cursorTrail.SetActive(true);
        }

        private void OnDisable()
        {
            transition.material.SetFloat("_FadingFade", 1);
            transition.material.SetFloat("_GaussianBlurFade", 1);
            SSHStyleManager.API.Outline.SetColorOutlineStrength(0.11f);
        }
    }
}