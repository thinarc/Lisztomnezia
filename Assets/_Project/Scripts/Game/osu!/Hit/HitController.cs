using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace _Project.Scripts.osu_.Hit
{
    public class HitController : MonoBehaviour, IPointerDownHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform pace;
        [SerializeField] private Animator paceAnim;
        [SerializeField] private Image paceCore;
        private Tween _paceScaleTween;

        [SerializeField, Space(5)] private TextMeshProUGUI nText;
        [SerializeField] private TextMeshProUGUI[] passText;

        [SerializeField, Space(5)] private float duration = 2f;
        [SerializeField] private float durationCheck = 2f;
        [SerializeField] private float checkOk = 2.4f;
        
        [Header("Tail")]
        [SerializeField, Space(5)] private bool isTailed;
        [SerializeField] private float tailDuration;
        private bool _tailed;
        private bool _dropped;

        [SerializeField] private RectTransform[] endTail;
        [SerializeField] private RectTransform tail;
        
        [SerializeField, Space(5)] private UnityEvent onClick;
        public void OnPointerDown(PointerEventData eventData) => onClick?.Invoke();

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_tailed) return;
            _dropped = true;
        }
        
        private CanvasGroup _group;
        private Animator _anim;

        private bool _run;
        private bool _miss;
        
        private void OnEnable()
        {
            _group = GetComponent<CanvasGroup>();
            _anim = GetComponent<Animator>();

            _group.DOFade(0, 0);
            paceCore.DOFade(0, 0);
            pace.DOScale(3, 0);
            ProcessButton();
        }

        private async void Miss()
        {
            _miss = true;
            OsuController.Instance.KickSlider();
            
            _anim.SetTrigger("miss");
            paceAnim.SetTrigger("miss");
            if (isTailed) await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            _paceScaleTween.Kill();
            pace.DOScale(pace.localScale.x * 1.02f, 0.2f).SetEase(Ease.OutSine);
            _run = false;
            
            if (!isTailed) return;
            tail.GetComponent<CanvasGroup>().DOFade(0, tailDuration / 6f).SetEase(Ease.InSine);
            endTail[0].GetComponent<CanvasGroup>().DOFade(0, tailDuration / 4f).SetEase(Ease.InSine);
        }

        public void Click()
        {
            if (!_run) return;
            
            if (pace.localScale.x > 0.95f && pace.localScale.x < durationCheck)
            {
                if (!isTailed)
                {
                    if (pace.localScale.x < checkOk) passText[0].gameObject.SetActive(true);
                    else passText[1].gameObject.SetActive(true);
                }
                
                _anim.SetTrigger("click");
                paceAnim.SetTrigger("click");
                _paceScaleTween.Kill();
                if (!isTailed)
                {
                    OsuController.Instance.RegenSlider();
                    pace.DOScale(pace.localScale.x * 1.02f, 0.4f).SetEase(Ease.InSine);
                }
                else Tail(pace.localScale.x);
            }
            else Miss();
        }

        public async void Tail(float paceScale)
        {
            _tailed = true;
            transform.DOLocalMove(endTail[0].anchoredPosition + endTail[1].anchoredPosition, tailDuration).SetEase(Ease.OutSine);
            pace.DOLocalMove(endTail[0].anchoredPosition + endTail[1].anchoredPosition, tailDuration).SetEase(Ease.OutSine);
            _paceScaleTween = pace.DOScale(pace.localScale.x * 1.4f, tailDuration / 3.33f + tailDuration / 3.076f).SetEase(Ease.InOutSine);
            tail.GetComponent<CanvasGroup>().DOFade(0, tailDuration / 2f).SetEase(Ease.InSine);
            endTail[0].GetComponent<CanvasGroup>().DOFade(0, tailDuration).SetEase(Ease.InSine);
            
            await UniTask.Delay(TimeSpan.FromSeconds(tailDuration / 3.33f));
            if (_dropped) Miss();
            
            await UniTask.Delay(TimeSpan.FromSeconds(tailDuration / 3.076f));
            
            if (paceScale < checkOk) passText[0].gameObject.SetActive(true);
            else passText[1].gameObject.SetActive(true);
            
            _anim.SetTrigger("drop");
            _tailed = false;
            if (!_miss) OsuController.Instance.RegenSlider();
        }

        public void SetNumber(int n) => nText.text = n.ToString();

        private void ProcessButton()
        {
            _group.DOFade(1, duration).SetEase(Ease.InOutSine);
            paceCore.DOFade(1, duration * 2f).SetEase(Ease.InQuad);
            _paceScaleTween = pace.DOScale(0.95f, duration).SetEase(Ease.InOutQuad).OnComplete(Miss);
            _run = true;
        }
    }
}