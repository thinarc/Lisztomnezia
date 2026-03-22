using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Dialog
{
    public class DialogController : MonoBehaviour
    {
        public static DialogController Instance { get; private set; }
        
        [SerializeField] private TextMeshProUGUI message;
        private CanvasGroup _canvas;

        private void Start()
        {
            Instance = this;
            
            _canvas = GetComponent<CanvasGroup>();
        }

        public async void ShowMessage(string text)
        {
            message.text = "";
            _canvas.DOFade(1, 0.4f).SetEase(Ease.OutExpo);
            message.text = text;

            await UniTask.Delay(text.Length * 200);
            await _canvas.DOFade(0, 0.4f).SetEase(Ease.OutExpo).AsyncWaitForCompletion();
            message.text = "";
        }
    }
}
