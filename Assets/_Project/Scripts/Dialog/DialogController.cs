using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.Utilities;
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
        
        [SerializeField] private TextMeshProUGUI personMessage;
        public bool personCall;

        [SerializeField, Space(5)] private bool world;
        
        private void Start()
        {
            Instances ??= new DialogController[2];
            if (!world) Instances[0] = this;
            else Instances[1] = this;
            
            _canvas = GetComponent<CanvasGroup>();

            if (LocalizationSettings.SelectedLocale != LocalizationSettings.AvailableLocales.Locales[1]) return;
            message.fontSize /= 2f;
            if (personMessage) personMessage.fontSize /= 2f;
        }

        public async UniTask ShowMessage(string text, Transform newZ = null, bool downY = false)
        {
            if (newZ != null)
            {
                var pos = transform.position;
                    
                if (downY) pos.y -= 0.42f;
                pos.z = newZ.position.z;
                
                transform.position = pos;
            }

            var m = message;
            if (personCall && personMessage)
            {
                m = personMessage;
                personCall = false;
            }
            
            m.text = "";
            _canvas.DOFade(1, 0.4f).SetEase(Ease.OutExpo);
            m.text = text;

            var delay = 60;
            if (newZ != null) delay = 200;

            var wait = text.Length * delay;
            Fade();

            async void Fade()
            {
                await UniTask.Delay(wait);
                _canvas.DOFade(0, 0.6f).SetEase(Ease.InExpo);
                m.text = "";
            }

            await UniTask.Delay(wait / 2);
        }
    }
}
