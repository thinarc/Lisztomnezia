using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace _Project.Scripts.Dialog
{
    public class DialogController : MonoBehaviour
    {
        public static DialogController[] Instances { get; private set; }
        
        [SerializeField] private TextMeshProUGUI message;
        private CanvasGroup _canvas;

        [SerializeField, Space(5)] private bool world;
        
        private void Start()
        {
            Instances ??= new DialogController[2];
            if (!world) Instances[0] = this;
            else Instances[1] = this;
            
            _canvas = GetComponent<CanvasGroup>();

            if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[1])
                message.fontSize /= 2f;
        }

        public async void ShowMessage(string text, Transform newZ = null, bool downY = false)
        {
            if (newZ != null)
            {
                var pos = transform.position;
                    
                if (downY) pos.y -= 0.42f;
                pos.z = newZ.position.z;
                
                transform.position = pos;
            }
            
            message.text = "";
            _canvas.DOFade(1, 0.4f).SetEase(Ease.OutExpo);
            message.text = text;

            var delay = 80;
            if (newZ != null) delay = 200;
            
            await UniTask.Delay(text.Length * delay);
            await _canvas.DOFade(0, 0.4f).SetEase(Ease.OutExpo).AsyncWaitForCompletion();
            message.text = "";
        }
    }
}
