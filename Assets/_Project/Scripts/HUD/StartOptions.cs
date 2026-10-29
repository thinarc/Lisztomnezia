using _Project.Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.HUD
{
    public class StartOptions : MonoBehaviour
    {
        [SerializeField] private CanvasGroup logo;
        [SerializeField] private Transform authorsParent;

        [SerializeField, Space(5)] private Slider bgSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider voiceSlider;
        
        [SerializeField, Space(5)] private TextMeshProUGUI bgText;
        [SerializeField] private TextMeshProUGUI sfxText;
        [SerializeField] private TextMeshProUGUI voiceText;
        
        [SerializeField, Space(5)] private TMP_Dropdown locale;
        
        private CanvasGroup _group;
        
        private bool _fill;
        private Vector3[] _authorsBeforeExit;
        private float _logoBeforeExit;
        private Vector3[] _authorsBeforePause;
        private float _logoBeforePause;

        public bool TitleIntroFinished
        {
            get
            {
                var animator = logo.GetComponent<Animator>();
                return animator == null || !animator.enabled ||
                    (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !animator.IsInTransition(0));
            }
        }

        private void Awake()
        {
            transform.localScale = Vector3.one;
            var canvas = GetComponent<Canvas>();
            canvas.sortingOrder = 900;
            _group = GetComponentInChildren<CanvasGroup>();
            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        public async UniTask PlayTitleExit()
        {
            _logoBeforeExit = logo.alpha;
            _authorsBeforeExit = new Vector3[authorsParent.childCount];
            var departure = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            departure.Join(logo.DOFade(0, 0.2f).SetEase(Ease.OutExpo));
            for (var i = 0; i < authorsParent.childCount; i++)
            {
                var author = authorsParent.GetChild(i);
                _authorsBeforeExit[i] = author.localPosition;
                departure.Join(author.DOLocalMoveZ(-21f, 0.4f).SetEase(Ease.OutExpo));
            }
            await departure.AsyncWaitForCompletion();
        }

        public void RestoreTitleAfterExit()
        {
            if (_authorsBeforeExit == null) return;
            // Finish the intro while hidden, then prevent it replaying when pause reactivates the title.
            var intro = logo.GetComponent<Animator>();
            if (intro != null)
            {
                intro.Play(intro.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
                intro.Update(0);
                intro.enabled = false;
            }
            // The parent also owns the author spheres; hide only its logo artwork child.
            var artwork = logo.transform.Find("Logo");
            if (artwork != null) artwork.gameObject.SetActive(false);
            logo.alpha = _logoBeforeExit;
            for (var i = 0; i < _authorsBeforeExit.Length; i++)
                authorsParent.GetChild(i).localPosition = _authorsBeforeExit[i];
            _authorsBeforeExit = null;
        }

        public void SetVisible(bool visible)
        {
            if (!visible) locale.Hide();
            if (!visible && !_up && _pressedIndex >= 0) OnPointUp(_pressedIndex);
            _up = true;
            _group.alpha = visible ? 1 : 0;
            _group.interactable = visible;
            _group.blocksRaycasts = visible;
            if (!visible)
            {
                RestorePauseDecorations();
                return;
            }
            
            _fill = true;
            bgSlider.value = SoundManager.Instance.BgVolume;
            sfxSlider.value = SoundManager.Instance.SfxVolume;
            voiceSlider.value = SoundManager.Instance.VoiceVolume;
            locale.SetValueWithoutNotify(UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.Locales.IndexOf(
                UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale));
            _fill = false;
        }

        public async UniTask OpenPause()
        {
            _logoBeforePause = logo.alpha;
            _authorsBeforePause = new Vector3[authorsParent.childCount];
            var departure = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            departure.Join(logo.DOFade(0, 0.2f).SetEase(Ease.OutExpo));
            for (var i = 0; i < authorsParent.childCount; i++)
            {
                var author = authorsParent.GetChild(i);
                _authorsBeforePause[i] = author.localPosition;
                departure.Join(author.DOLocalMoveZ(-21f, 0.4f).SetEase(Ease.OutExpo));
            }
            var settings = FadeVisible(true, 0.4f);
            await departure.AsyncWaitForCompletion();
            await settings;
        }

        private void RestorePauseDecorations()
        {
            if (_authorsBeforePause == null) return;
            logo.alpha = _logoBeforePause;
            for (var i = 0; i < _authorsBeforePause.Length; i++)
                authorsParent.GetChild(i).localPosition = _authorsBeforePause[i];
            _authorsBeforePause = null;
        }

        public async UniTask FadeVisible(bool visible, float duration)
        {
            if (visible)
            {
                SetVisible(true);
                _group.alpha = 0;
            }
            else PrepareToClose();
            _group.interactable = false;
            _group.blocksRaycasts = visible;
            await _group.DOFade(visible ? 1 : 0, duration).SetEase(Ease.InOutSine)
                .SetUpdate(true).SetLink(gameObject).AsyncWaitForCompletion();
            if (this == null) return;
            if (visible) _group.interactable = true;
            else SetVisible(false);
        }

        public void PrepareToClose()
        {
            locale.Hide();
            _group.interactable = false;
            if (!_up && _pressedIndex >= 0) OnPointUp(_pressedIndex);
        }

        public void OnLocaleChanged() => LangManager.InitLocale(locale.value);

        public void OnBackgroundChanged()
        {
            if (_fill) return;
            SoundManager.Instance.BgVolume = bgSlider.value;
        }

        public void OnSfxChanged()
        {
            if (_fill) return;
            SoundManager.Instance.SfxVolume = sfxSlider.value;
        }

        public void OnVoiceChanged()
        {
            if (_fill) return;
            SoundManager.Instance.VoiceVolume = voiceSlider.value;
        }

        public async void OnPointDown(int i)
        {
            _up = false;
            _pressedIndex = i;
            
            var text = bgText;
            var vol = SoundManager.Instance.BgVolume;

            if (i == 1)
            {
                text = sfxText;
                vol = SoundManager.Instance.SfxVolume;
            }
            else if (i == 2)
            {
                text = voiceText;
                vol = SoundManager.Instance.VoiceVolume;
            }
            
            text.text = (vol * 100).ToString("F0") + "%";
            
            text.transform.DOMoveY(text.transform.position.y - 5f, 0.1f).SetEase(Ease.OutSine).SetUpdate(true);
            text.transform.DOScale(text.transform.localScale.x * 1.1f, 0.1f).SetEase(Ease.OutSine).SetUpdate(true);

            while (true)
            {
                vol = SoundManager.Instance.BgVolume;
                if (i == 1) vol = SoundManager.Instance.SfxVolume;
                else if (i == 2) vol = SoundManager.Instance.VoiceVolume;
                text.text = (vol * 100).ToString("F0") + "%";
                await UniTask.Yield();
                if (_up) break;
            }
        }

        private bool _up = true;
        private int _pressedIndex = -1;
        
        public void OnPointUp(int i)
        {
            if (_up || i != _pressedIndex) return;
            _up = true;
            _pressedIndex = -1;
            
            var text = bgText;
            if (i == 1) text = sfxText;
            else if (i == 2) text = voiceText;
            
            text.transform.DOMoveY(text.transform.position.y + 5f, 0.1f).SetEase(Ease.InSine).SetUpdate(true);
            text.transform.DOScale(text.transform.localScale.x / 1.1f, 0.1f).SetEase(Ease.InSine).SetUpdate(true);
            
            if (i == 0) bgText.text = "Background";
            else if (i == 1) sfxText.text = "SFX";
            else if (i == 2) voiceText.text = "Voice";
        }
    }
}
