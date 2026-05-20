using System;
using System.Collections.Generic;
using _Project.Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

                await UniTask.Yield();

                if (Mathf.Abs(bgSlider.value - SoundManager.Instance.BgVolume) < 0.01f && Mathf.Abs(sfxSlider.value - SoundManager.Instance.SfxVolume) < 0.01f)
                {
                    bgSlider.value = SoundManager.Instance.BgVolume;
                    sfxSlider.value = SoundManager.Instance.SfxVolume;
                    break;
                }
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
    }
}