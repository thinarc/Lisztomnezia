using UnityEngine;

namespace SplineAS_DEMO
{
    public class DemoController : MonoBehaviour
    {
        private bool _hintEnabled = false;
        [SerializeField] private GameObject[] sasHints;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Application.targetFrameRate = 60;
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                _hintEnabled = !_hintEnabled;
                foreach (GameObject hint in sasHints)
                {
                    hint.SetActive(_hintEnabled);
                }
            }
        }

        void OnGUI()
        {
            Vector2 nativeSize = new Vector2(640, 480);
            GUIStyle style = new GUIStyle ();
            style.fontSize = (int)(9.0f * ((float)Screen.width / (float)nativeSize.x));

            GUI.Label(new Rect(10, 10, 200, 40), "" +
                                                 "Spline Audio Source Demo\n" +
                                                 "H -> Toggle Spline Audio Source Hints\n" +
                                                 "", style);
            
            GUI.Label(new Rect(Screen.width - 200, 10, 100, 40), $"FPS: {(int)(1.0/Time.deltaTime)} / 60", style);
        }
    }
}

