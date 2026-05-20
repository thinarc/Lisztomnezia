using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.HUD
{
    public class StartOptions : MonoBehaviour
    {
        [SerializeField] private CanvasGroup logo;
        [SerializeField] private Transform authorsParent;
        
        private CanvasGroup _group;
        private List<Transform> _authors = new();

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
            
            await _group.DOFade(1, 0.4f).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
            _group.interactable = true;
            _group.blocksRaycasts = true;

            await UniTask.Delay(20000);
        }
    }
}