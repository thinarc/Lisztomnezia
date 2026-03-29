using Cysharp.Threading.Tasks;
using SmnStyleHardline.Core;
using TMPEffects.Components;
using TMPro;
using UnityEditor;
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

        public TextMeshProUGUI escText;
        private float _esc = 2f;
        private bool waitDel = false;

        private async void Update()
        {
            if (Input.GetKey(KeyCode.Escape)) _esc -= Time.deltaTime;
            else if (_esc < 2f) _esc += Time.deltaTime * 4;
            
            if (_esc < 2f && escText.text != ">>>")
            {
                escText.text = ">>>";
                escText.GetComponent<TMPWriter>().enabled = false;
            }

            if (_esc < 1.65f && escText.text != ">>") escText.text = ">>";

            if (_esc < 0f && escText.text != ">" && !waitDel)
            {
                if (SceneManager.GetActiveScene().buildIndex == 2)
                {
                    escText.text = ">";
                    waitDel = true;
                    await UniTask.Delay(200);
                    SceneManager.LoadScene(1);
                    waitDel = false;
                    escText.text = "";
                    _esc = 2f;
                    return;
                }
                
                escText.text = ">";
                await UniTask.Delay(200);
                waitDel = true;
                Application.Quit();
                escText.text = "<<";
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }

            if (_esc >= 2f && escText.text != "")
            {
                escText.text = "";
                escText.GetComponent<TMPWriter>().enabled = true;
            }
        }
    }
}