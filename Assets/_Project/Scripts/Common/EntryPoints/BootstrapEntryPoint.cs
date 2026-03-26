using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Common.EntryPoints
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        public static BootstrapEntryPoint Instance { get; private set; }
        
        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            SceneManager.LoadScene(2);
        }
    }
}