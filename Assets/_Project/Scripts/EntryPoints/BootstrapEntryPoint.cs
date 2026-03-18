using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.EntryPoints
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
            SceneManager.LoadScene(1);
        }
    }
}