using SmnStyleHardline.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Common.EntryPoints
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        public static BootstrapEntryPoint Instance { get; private set; }
        
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            SSHStyleManager.API.Outline.SetColorOutlineStrength(3f);
            
            Instance = this;
            DontDestroyOnLoad(this);
            SceneManager.LoadScene(1);
        }
    }
}