using System;
using System.Collections.Generic;
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
        private List<Transform> _authors = new();
        
        private bool _fill;

        private void Start()
        {
            _group = GetComponentInChildren<CanvasGroup>();
            _group.DOFade(0, 0);
            _group.interactable = false;
            _group.blocksRaycasts = false;
            
            for (var i = 0; i < authorsParent.childCount; i++) _authors.Add(authorsParent.GetChild(i));
        }
        
        public async UniTask CheckOptions()
        {
            await UniTask.Delay(2000);
            
            logo.DOFade(0, 0.2f).SetEase(Ease.OutExpo);
            _authors.ForEach(a => a.DOLocalMoveZ(-21f, 0.4f).SetEase(Ease.OutExpo));
            
            _fill = true;
            bgSlider.value = 0f;
            sfxSlider.value = 0f;
            voiceSlider.value = 0f;
            
            _group.DOFade(1, 0.4f).SetEase(Ease.InOutSine);
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            
            while (true)
            {
                bgSlider.value = Mathf.Lerp(
                    bgSlider.value,
                    SoundManager.Instance.BgVolume,
                    Time.deltaTime * 2);

                sfxSlider.value = Mathf.Lerp(
                    sfxSlider.value,
                    SoundManager.Instance.SfxVolume,
                    Time.deltaTime * 2);
                
                voiceSlider.value = Mathf.Lerp(
                    voiceSlider.value,
                    SoundManager.Instance.VoiceVolume,
                    Time.deltaTime * 2);

                await UniTask.Yield();

                if (!(Mathf.Abs(bgSlider.value - SoundManager.Instance.BgVolume) < 0.01f) ||
                    !(Mathf.Abs(sfxSlider.value - SoundManager.Instance.SfxVolume) < 0.01f) ||
                    !(Mathf.Abs(voiceSlider.value - SoundManager.Instance.VoiceVolume) < 0.01f)) continue;
                bgSlider.value = SoundManager.Instance.BgVolume;
                sfxSlider.value = SoundManager.Instance.SfxVolume;
                voiceSlider.value = SoundManager.Instance.VoiceVolume;
                break;
            }
            
            _group.interactable = true;
            _group.blocksRaycasts = true;
            _fill = false;

            await UniTask.Delay(20000);
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
            
            text.transform.DOMoveY(text.transform.position.y - 5f, 0.1f).SetEase(Ease.OutSine);
            text.transform.DOScale(text.transform.localScale.x * 1.1f, 0.1f).SetEase(Ease.OutSine);

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

        private bool _up;
        
        public void OnPointUp(int i)
        {
            _up = true;
            
            var text = bgText;
            if (i == 1) text = sfxText;
            else if (i == 2) text = voiceText;
            
            text.transform.DOMoveY(text.transform.position.y + 5f, 0.1f).SetEase(Ease.InSine);
            text.transform.DOScale(text.transform.localScale.x / 1.1f, 0.1f).SetEase(Ease.InSine);
            
            if (i == 0) bgText.text = "Background";
            else if (i == 1) sfxText.text = "SFX";
            else if (i == 2) voiceText.text = "Voice";
        }
    }
}