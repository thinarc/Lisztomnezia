using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Diary
{
    public class DiaryController : MonoBehaviour
    {
        public static DiaryController Instance { get; private set; }
        
        [SerializeField] private Rigidbody cursor;
        [SerializeField] private float cursorSpeed;
        private Vector2 _cursorDir;

        [SerializeField] private TrailRenderer[] drawDots;
        [SerializeField] private float drawSpeedMulti = 1f;
        [SerializeField] private Vector3 trailOffset = new (-2f, 0f, 0f);
        
        private Animator _anim;
        
        private bool _isOpen;
        private bool _isMore;
        
        private float _z;

        private bool _completed;

        private void Start()
        {
            Instance = this;
            _anim = GetComponent<Animator>();
            
            _z = cursor.position.z;
            TrailEmitting(false);
        }

        public void FillDraw()
        {
            var fire = Input.GetKey(KeyCode.Mouse0);
            _anim.SetBool("Draw", fire);
            if (fire)
            {
                drawSpeedMulti += Time.deltaTime * 2f;
                drawSpeedMulti = Mathf.Clamp(drawSpeedMulti, 1f, 12f);
                _anim.SetFloat("DrawSpeed", drawSpeedMulti);
                if (drawSpeedMulti >= 12f && !_completed)
                {
                    _completed = true;
                    print("complete draw");
                }
            }
        }

        private void Update()
        {
            var fire = Input.GetKey(KeyCode.Mouse0);

            if (Input.GetKeyDown(KeyCode.Mouse0)) TrailEmitting(true);
            else if (Input.GetKeyUp(KeyCode.Mouse0)) TrailEmitting(false);
            if (!fire)
            {
                drawSpeedMulti -= Time.deltaTime * 4f;
                drawSpeedMulti = Mathf.Clamp(drawSpeedMulti, 1f, drawSpeedMulti);
                _anim.SetFloat("DrawSpeed", drawSpeedMulti);
                _anim.SetBool("Draw", false);
            }

            _cursorDir = _isMore ? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) : Vector2.zero;
            
            if (_isOpen && Input.GetKeyDown(KeyCode.Mouse1))
            {
                _anim.SetTrigger("Transition");
                _isMore = !_isMore;
                _z = cursor.position.z;
                TrailEmitting(false);
                
                if (!_isMore) OffsetTrail(false);
                else OffsetTrail(true);
            }
        }

        private void TrailEmitting(bool emit)
        {
            drawDots.ForEach(dot => dot.emitting = emit);
        }

        private async void OffsetTrail(bool inversedOffset)
        {
            await UniTask.Delay(60);
            
            var offset = trailOffset;
            if (inversedOffset) offset = -offset;

            var t = 0;
            while (t < 42)
            {
                foreach (var trail in drawDots)
                {
                    var count = trail.positionCount;
                    if (count == 0) return;

                    var positions = new Vector3[count];
                    trail.GetPositions(positions);

                    for (var i = 0; i < count; i++)
                        positions[i] += offset / 42;

                    trail.SetPositions(positions);
                    t++;

                    await UniTask.NextFrame();
                }
            }
        }

        private void FixedUpdate()
        {
            cursor.linearVelocity = new Vector3(_cursorDir.x, _cursorDir.y, 0f) * cursorSpeed;
            
            var pos = cursor.position;

            var cam = Camera.main;
            if (cam == null) return;
            var min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.WorldToScreenPoint(pos).z));
            var max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.WorldToScreenPoint(pos).z));

            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);
            pos.z = _z;

            cursor.position = pos;
        }

        public void EnterDiary()
        {
            _anim.SetTrigger("Enter");
            _isOpen = true;
        }
    }
}