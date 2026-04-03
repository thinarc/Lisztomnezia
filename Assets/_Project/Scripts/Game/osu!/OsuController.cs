using System;
using System.Globalization;
using _Project.Scripts.Managers;
using _Project.Scripts.osu_.Align;
using _Project.Scripts.osu_.Hit;
using _Project.Scripts.Sound;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EvolveGames;
using Sirenix.OdinInspector;
using SmnStyleHardline.Core;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI lenghtNowText;
        [SerializeField] private TextMeshProUGUI lenghtMaxText;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private CanvasGroup user;

        [SerializeField, Space(5)] private RectTransform hitsParent;

        private float _slider;
        private bool _fill;
        private bool _fillCalled;

        private bool _fail;
        private bool _pass;

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
            
            repeatText.gameObject.SetActive(false);
        }

        public TextMeshProUGUI defeatText;
        public TextMeshProUGUI repeatText;

        public Image lvlImage;
        public Sprite[] lvlSprites;
        
        public async UniTask RunGame(int level)
        {
            lvlImage.sprite = lvlSprites[level - 1];
            
            await StartTransition();
            _fill = true;
            
            SoundManager.Instance.MusicLoop.SetPauseAmb(true);
            SoundManager.Instance.OsuCase.StartLevel(level);
            
            var desc = SoundManager.Instance.OsuCase.GetDesc()[1];
            lenghtSlider.maxValue = desc;
            lenghtMaxText.text = FormatTime((int)desc).ToString(CultureInfo.CurrentCulture);
            lenghtSlider.value = 0;
            
            await EndTransition();
            _run = true;

            while (true)
            {
                if (_fail)
                {
                    SoundManager.Instance.OsuCase.StopLevel();
                    await WaitRepeat();
                    
                    _hitLimit = 9;
                    _hitCounts = 0;
                    
                    slider.value = _slider;
                    _fill = false;
            
                    lenghtSlider.value = 0;
                    speedSlider.value = 0.5f;
                    
                    defeatText.gameObject.SetActive(true);
                    defeatText.transform.DOScale(1f - 0.025f, 0).OnComplete(() =>
                    {
                        defeatText.transform.DOScale(1f, 1f).SetEase(Ease.OutSine);
                    });
                    await defeatText.DOFade(0, 0).AsyncWaitForCompletion();
                    defeatText.DOFade(1, 1f).SetEase(Ease.OutSine);

                    await UniTask.Delay(2000);
                    
                    await defeatText.DOFade(0, 0.4f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                    
                    repeatText.gameObject.SetActive(true);
                    repeatText.transform.DOScale(1.025f, 0).OnComplete(() =>
                    {
                        repeatText.transform.DOScale(1f, 1f).SetEase(Ease.OutSine);
                    });
                    await repeatText.DOFade(0, 0).AsyncWaitForCompletion();
                    await repeatText.DOFade(1, 0.4f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                    
                    SetReadSpectrum(false);

                    var power = 0f;
                    var res = false;
                    while (true)
                    {
                        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0))
                        {
                            power += Time.deltaTime * 0.5f * 0.75f;
                            res = SoundManager.Instance.OsuCase.CaseChange(Time.deltaTime * 0.5f * 0.75f / 15f);
                        }
                        else if (power > 0)
                        {
                            power -= Time.deltaTime * 3;
                            res = SoundManager.Instance.OsuCase.CaseChange(-Time.deltaTime * 3 / 15f);
                        }
                        repeatText.transform.localScale = Vector3.one * Mathf.Abs(1f + power * 0.1f);
                        if (power > 2f)
                        {
                            repeatText.transform.DOScale(2f, 1f).SetEase(Ease.OutSine);
                            await repeatText.DOFade(0, 0.2f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                            repeatText.gameObject.SetActive(false);
                            break;
                        }
                        
                        await UniTask.Yield();
                    }
                    
                    _fill = true;

                    if (res) await UniTask.Delay(500);
                    SoundManager.Instance.OsuCase.RepLevel();
                    await RepeatTransition();
                    for (var i = 0; i < hitsParent.childCount; i++) Destroy(hitsParent.GetChild(i).gameObject);
                    SetReadSpectrum(true);
                    _fail = false;
                    _slider = 0;
                    _fillCalled = false;
                }
                
                else if (_pass && _run)
                {
                    _run = false;
                    _pass = false;

                    SoundManager.Instance.OsuCase.PassLevel();
                    await WaitRepeat();
                    
                    _hitLimit = 9;
                    _hitCounts = 0;
                    
                    slider.value = _slider;
                    _fill = false;
            
                    lenghtSlider.value = 0;
                    speedSlider.value = 0.5f;
                    
                    // defeatText.gameObject.SetActive(true);
                    // defeatText.transform.DOScale(1f - 0.025f, 0).OnComplete(() =>
                    // {
                    //     defeatText.transform.DOScale(1f, 1f).SetEase(Ease.OutSine);
                    // });
                    // await defeatText.DOFade(0, 0).AsyncWaitForCompletion();
                    // defeatText.DOFade(1, 1f).SetEase(Ease.OutSine);
                    //
                    // await UniTask.Delay(2000);
                    //
                    // await defeatText.DOFade(0, 0.4f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                    
                    // repeatText.gameObject.SetActive(true);
                    // repeatText.transform.DOScale(1.025f, 0).OnComplete(() =>
                    // {
                    //     repeatText.transform.DOScale(1f, 1f).SetEase(Ease.OutSine);
                    // });
                    // await repeatText.DOFade(0, 0).AsyncWaitForCompletion();
                    // await repeatText.DOFade(1, 0.4f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                    
                    SetReadSpectrum(false);

                    // var power = 0f;
                    // var res = false;
                    // while (true)
                    // {
                    //     if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0))
                    //     {
                    //         power += Time.deltaTime * 0.5f * 0.75f;
                    //         res = SoundManager.Instance.OsuCase.CaseChange(Time.deltaTime * 0.5f * 0.75f / 15f);
                    //     }
                    //     else if (power > 0)
                    //     {
                    //         power -= Time.deltaTime * 3;
                    //         res = SoundManager.Instance.OsuCase.CaseChange(-Time.deltaTime * 3 / 15f);
                    //     }
                    //     repeatText.transform.localScale = Vector3.one * Mathf.Abs(1f + power * 0.1f);
                    //     if (power > 2f)
                    //     {
                    //         repeatText.transform.DOScale(2f, 1f).SetEase(Ease.OutSine);
                    //         await repeatText.DOFade(0, 0.2f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                    //         repeatText.gameObject.SetActive(false);
                    //         break;
                    //     }
                    //     
                    //     await UniTask.Yield();
                    // }
                    
                    // _fill = true;

                    // if (res) await UniTask.Delay(500);
                    // SoundManager.Instance.OsuCase.RepLevel();
                    await ExitTransition();
                    for (var i = 0; i < hitsParent.childCount; i++) Destroy(hitsParent.GetChild(i).gameObject);
                    SetReadSpectrum(true);
                    _fail = false;
                    _slider = 0;
                    _fillCalled = false;
                    
                    player.enabled = true;
                    
                    break;
                }
                
                await UniTask.Yield();
            }
        }

        public async void SetReadSpectrum(bool read)
        {
            disableBass = !read;
            disableSnare = !read;
        }
        
        public void OnBass()
        {
            if (_fail) return;
            if (!_run || disableBass) return;
            _bassValue = 1f;
        }

        public void OnSnare()
        {
            if (_fail) return;
            if (!_run || disableSnare) return;
            _snareValue = 1f;
        }

        [Button] public void KickSlider()
        {
            if (_fail) return;
            _slider -= 0.2f;
            _slider = Mathf.Clamp(_slider, 0.001f, 1f);
            slider.DOValue(_slider, 0.4f).SetEase(Ease.OutSine);
            if (_slider <= 0.001f) _fail = true;
        }
        
        [Button] public void RegenSlider(bool meh)
        {
            if (_fail) return;
            if (meh) _slider += 0.025f;
            else _slider += 0.05f;
            _slider = Mathf.Clamp(_slider, 0.001f, 1f);
            slider.DOValue(_slider, 0.4f).SetEase(Ease.OutSine);
        }
        
        string FormatTime(int ms)
        {
            int totalSeconds = ms / 1000;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes}:{seconds:00}";
        }
        
        private void Update()
        {
            var desc = SoundManager.Instance.OsuCase.GetDesc();
            lenghtSlider.value = desc[0];
            lenghtNowText.text = FormatTime((int)desc[0]).ToString(CultureInfo.CurrentCulture);

            if (lenghtSlider.value / lenghtSlider.maxValue >= 1f && _run) _pass = true;
            
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
                var hit = Instantiate(bassPrefabs[0], hitsParent);
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
                var hit = Instantiate(snarePrefabs[prefabIndex], hitsParent);
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

        private async UniTask WaitRepeat()
        {
            user.DOFade(0, 0.4f).SetEase(Ease.InOutSine);
            hitsParent.GetComponent<CanvasGroup>().DOFade(0, 0.4f).SetEase(Ease.InOutSine);
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            cursorTrail.SetActive(false);
            
            transition.material.SetFloat("_FadingFade", 0);
            transition.material.SetFloat("_GaussianBlurFade", 0);
            
            SSHStyleManager.API.Outline.SetColorOutlineStrength(0.11f);
            
            GetComponent<Image>().material = _oldMat;
            transition.DOFade(1, 0.4f).SetEase(Ease.InOutSine);
            await UniTask.Delay(TimeSpan.FromSeconds(0.08f));
            _canvas.DOFade(0, 0.1f).SetEase(Ease.OutQuad);
            
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
        }

        private Material _oldMat;
        
        private async UniTask RepeatTransition()
        {
            user.DOFade(1, 0.2f).SetEase(Ease.InOutSine);
            hitsParent.GetComponent<CanvasGroup>().DOFade(1, 0.2f).SetEase(Ease.InOutSine);
            
            SoundManager.Instance.OsuCase.RepLevel();
            
            SSHStyleManager.API.Outline.SetColorOutlineStrength(3f);
            
            GetComponent<Image>().material = transition.material;
            _canvas.DOFade(1, 0.4f).SetEase(Ease.InOutSine);
            await UniTask.Delay(TimeSpan.FromSeconds(0.08f));
            transition.DOFade(0, 0.1f).SetEase(Ease.OutQuad);
            
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
        
        private async UniTask ExitTransition()
        {
            // user.DOFade(1, 0.2f).SetEase(Ease.InOutSine);
            // hitsParent.GetComponent<CanvasGroup>().DOFade(1, 0.2f).SetEase(Ease.InOutSine);
            
            // SoundManager.Instance.OsuCase.RepLevel();
            
            // SSHStyleManager.API.Outline.SetColorOutlineStrength(3f);
            
            GetComponent<Image>().material = transition.material;
            transition.DOFade(1, 0.4f).SetEase(Ease.InOutSine);
            await UniTask.Delay(TimeSpan.FromSeconds(0.08f));
            _canvas.DOFade(0, 0.1f).SetEase(Ease.OutQuad);
            
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
            
            cursorTrail.SetActive(false);
            
            transition.DOFade(0, 0.1f).SetEase(Ease.OutQuad);
        }
        
        private async UniTask StartTransition()
        {
            user.DOFade(1, 0.2f).SetEase(Ease.InOutSine);
            hitsParent.GetComponent<CanvasGroup>().DOFade(1, 0.2f).SetEase(Ease.InOutSine);
            
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

            var image = GetComponent<Image>();
            _oldMat = image.material;
            image.material = transition.material;
            _canvas.DOFade(1, 0.4f).SetEase(Ease.InOutSine);
            await UniTask.Delay(TimeSpan.FromSeconds(0.08f));
            transition.DOFade(0, 0.1f).SetEase(Ease.OutQuad);
            
            SSHStyleManager.API.Outline.SetColorOutlineStrength(3f);
            cursorTrail.SetActive(true);
        }

        private async UniTask EndTransition()
        {
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
        }
    }
}