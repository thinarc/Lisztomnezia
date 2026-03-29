using _Project.Scripts.HUD;
using _Project.Scripts.Managers;
using Cysharp.Threading.Tasks;
using SmnStyleHardline.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Common.EntryPoints
{
    public class LogoEntryPoint : MonoBehaviour
    {
        [SerializeField] private StartOptions startOptions;
        
        private async void Start()
        {
            await new LogoManager().ShowLogo();
            await startOptions.CheckOptions();
            
            await UniTask.WaitUntil(() => Input.GetKey(KeyCode.Space));
            SSHStyleManager.API.Outline.SetColorOutlineStrength(0.11f);
            // SceneManager.LoadScene(2);
        }
    }
}
