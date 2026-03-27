using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.osu_
{
    public class OsuController : MonoBehaviour
    {
        public static OsuController Instance { get; private set; }

        [SerializeField] private GameObject gameScreen;
        private RawImage _raw;

        private void Start()
        {
            Instance = this;
            
            _raw = GetComponent<RawImage>();
            _raw.material.SetFloat("_FadingFade", 0);
        }
        
        public async void RunGame()
        {
            await UniTask.Delay(500);
            
            gameScreen.SetActive(true);
            await UniTask.NextFrame();
            gameScreen.SetActive(false);
            
            await UniTask.WaitWhile(() =>
            {
                var val = Mathf.MoveTowards(_raw.material.GetFloat("_FadingFade"), 1, Time.deltaTime * Random.Range(0.4f, 0.7f));
                _raw.material.SetFloat("_FadingFade", val);
                return !Mathf.Approximately(val, 1);
            });
        }
        
        // private async void OnPrepared(VideoPlayer vp)
        // {
        //     await SoundManager.instance.PrepareFade();
        //     var group = GetComponent<CanvasGroup>();
        //     group.blocksRaycasts = true;
        //     group.interactable = true;
        //     group.DOFade(1, 0.4f).SetEase(Ease.InOutSine);
        //     SoundManager.instance.PlayAfterPrepare(clip);
        //     vp.Play();
        //
        //     player.loopPointReached += async _ =>
        //     {
        //         SoundManager.instance.ReturnFade();
        //         await UniTask.Delay(600);
        //         ShowBye();
        //         await UniTask.WaitWhile(() =>
        //         {
        //             var val = Mathf.MoveTowards(raw.material.GetFloat("_FadingFade"), 1, Time.deltaTime * Random.Range(0.07f, 0.24f));
        //             raw.material.SetFloat("_FadingFade", val);
        //             return !Mathf.Approximately(val, 1);
        //         });
        //
        //         async void ShowBye()
        //         {
        //             await UniTask.Delay(940);
        //             leavetext.alpha = 0;
        //             leavetext.gameObject.SetActive(true);
        //             leavetext.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).From(0.82f);
        //             leavetext.DOFade(1, 0.4f).SetEase(Ease.OutBack);
        //         }
        //     };
        // }
    }
}