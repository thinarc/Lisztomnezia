using _Project.Scripts.HUD;
using _Project.Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SmnStyleHardline.Core;
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
            await new LogoManager().ShowLogo();
            await startOptions.CheckOptions();
            
            xClick.DOFade(0, 0f).OnComplete(() => xClick.DOFade(1, 1f).SetEase(Ease.OutExpo));
            
            xClick.text = "(3x)";
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0));
            xClick.text = "(3x)";
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0));
            xClick.text = "(2x)";
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0));
            xClick.text = "(1x)";
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0));
            xClick.text = "(0x)";
            SSHStyleManager.API.Outline.SetColorOutlineStrength(0.11f);
            
            SceneManager.LoadScene(2);
        }
    }
}
