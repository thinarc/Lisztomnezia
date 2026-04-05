using UnityEngine;

namespace _Project.Scripts.Diary
{
    public class DiaryController : MonoBehaviour
    {
        public static DiaryController Instance { get; private set; }
        
        [SerializeField] private Transform cursor;
        private Vector3 _offset;
        private float _zCoord;
        
        private Animator _anim;
        
        private bool _isOpen;

        private void Start()
        {
            Instance = this;
            
            _anim = GetComponent<Animator>();
        }

        private void Update()
        {
            var fire = Input.GetKey(KeyCode.Mouse0);
            _anim.SetBool("Draw", fire);
            
            _zCoord = Camera.main.WorldToScreenPoint(cursor.position).z;
            _offset = cursor.position - GetMouseWorldPos();
            var pos = GetMouseWorldPos() + _offset;
            pos.z = cursor.position.z;
            cursor.position = pos;
            
            if (_isOpen && Input.GetKeyDown(KeyCode.Mouse1))
            {
                _anim.SetTrigger("Transition");
            }
        }

        public void EnterDiary()
        {
            _anim.SetTrigger("Enter");
            _isOpen = true;
        }
        
        private Vector3 GetMouseWorldPos()
        {
            var mousePoint = Input.mousePosition;
            mousePoint.z = _zCoord;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }
    }
}