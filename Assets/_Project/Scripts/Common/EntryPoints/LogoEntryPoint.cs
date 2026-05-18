using _Project.Scripts.Effects;
using _Project.Scripts.HUD;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Common.EntryPoints
{
    public class LogoEntryPoint : MonoBehaviour
    {
        [SerializeField] private StartOptions startOptions;
        
        public TextMeshProUGUI xClick;
        
        private async void Start()
        {
            // xClick.DOFade(0, 0f).OnComplete(() => xClick.DOFade(1, 1f).SetEase(Ease.OutExpo));
            // xClick.text = "(3x)";
            // await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0));
            // xClick.text = "(3x)";
            // await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0));
            // xClick.text = "(2x)";
            // await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0));
            // xClick.text = "(1x)";
            // await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0));
            // xClick.text = "(0x)";
            
            Cursor.visible = true;
            
            await startOptions.CheckOptions();
            DontDestroyOnLoad(this);

            Cursor.visible = false;
            
            OutlineMover.Initialize(0.11f);
            SceneManager.LoadScene(2);
        }
    }
}
