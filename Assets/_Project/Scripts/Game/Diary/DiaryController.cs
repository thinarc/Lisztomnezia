using _Project.Scripts.Effects;
using _Project.Scripts.Sense;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Diary
{
    public class DiaryController : MonoBehaviour
    {
        public static DiaryController Instance { get; private set; }
        
        [SerializeField] private Rigidbody cursor;
        [SerializeField] private float cursorSpeed;
        private float _startCursorSpeed;
        private Vector2 _cursorDir;

        [SerializeField] private TrailRenderer[] drawDots;
        [SerializeField] private float drawSpeedMulti = 1f;
        [SerializeField] private Vector3 trailOffset = new (-2f, 0f, 0f);
        [SerializeField] private float[] dotTimes;
        [SerializeField, Min(0.01f)] private float organSketchSize = 0.9f;
        [SerializeField] private Material organInkMaterial;
        [SerializeField] private Material organSplatterMaterial;
        private OrganSketch _organSketch;
        
        private Animator _anim;
        
        private bool _isOpen;
        private bool _isMore;
        
        private float _z;

        private System.Action<float> _onDrawProgress;
        private bool _completed;
        private bool _exit;

        private void Start()
        {
            Instance = this;
            if (_Project.Scripts.Sense.SenseManager.Instance == null)
                gameObject.AddComponent<_Project.Scripts.Sense.SenseManager>();
            _anim = GetComponent<Animator>();
            
            _z = cursor.position.z;
            TrailEmitting(false);
            
            _startCursorSpeed = cursorSpeed;
        }
        
        private void Update()
        {
            if (_Project.Scripts.Common.EntryPoints.BootstrapEntryPoint.IsPaused) return;

            if (Input.GetKeyDown(KeyCode.W)) _anim.SetBool("Walk", true);
            else if (Input.GetKeyUp(KeyCode.W)) _anim.SetBool("Walk", false);
            
            if (Input.GetKeyDown(KeyCode.LeftShift)) _anim.SetFloat("RunMultiplier", 1.6f);
            else if (Input.GetKeyUp(KeyCode.LeftShift)) _anim.SetFloat("RunMultiplier", 1f);
            
            if (Input.GetKeyDown(KeyCode.Mouse0)) TrailEmitting(true);
            else if (Input.GetKeyUp(KeyCode.Mouse0)) TrailEmitting(false);
            if (!Input.GetKey(KeyCode.Mouse0) || !_drawZone)
            {
                drawSpeedMulti -= Time.deltaTime * 4f;
                drawSpeedMulti = Mathf.Clamp(drawSpeedMulti, 1f, 12f);
                _anim.SetFloat("DrawSpeed", Mathf.Clamp(drawSpeedMulti, 1f, 16f));
                _anim.SetBool("Draw", false);
            }

            if (_isOpen && !_completed)
                OutlineMover.SetSketchProgress(Mathf.InverseLerp(1f, 12f, drawSpeedMulti));

            TrailEmitting(Input.GetKey(KeyCode.Mouse0) && _drawZone);
            _cursorDir = _isOpen ? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) : Vector2.zero;
        }

        public async void FillDraw()
        {
            if (!_isOpen || _completed) return;
            var fire = Input.GetKey(KeyCode.Mouse0);
            _anim.SetBool("Draw", fire);
            _drawZone = true;
            cursorSpeed = _startCursorSpeed;
            
            if (!fire) return;
            drawSpeedMulti += Time.deltaTime * 5.5f;
            drawSpeedMulti = Mathf.Clamp(drawSpeedMulti, 1f, 12f);
            _anim.SetFloat("DrawSpeed", Mathf.Clamp(drawSpeedMulti, 1f, 16f));
            _onDrawProgress?.Invoke(Mathf.InverseLerp(1f, 12f, drawSpeedMulti));
            if (drawSpeedMulti >= 12f && !_completed)
            {
                _anim.SetTrigger("Rip");
                _completed = true;
                _organSketch?.Update(1f);
                _onDrawProgress?.Invoke(1f);
                OutlineMover.CatchRabbit();
                
                for (var i = 0; i < drawDots.Length; i++)
                {
                    dotTimes[i] = drawDots[i].time;
                }

                // Fade all strokes together before the next sketch can begin.
                for (var step = 0; step < 6 && !_exit; step++)
                {
                    foreach (var dot in drawDots) dot.time *= 0.6f;
                    _organSketch?.Fade(1f - (step + 1f) / 6f);
                    await UniTask.Delay(50);
                }
            }
        }

        public void Completed() => _exit = true;

        private void LateUpdate()
        {
            if (_isOpen && !_completed && !_Project.Scripts.Common.EntryPoints.BootstrapEntryPoint.IsPaused)
            {
                if (_drawZone && Input.GetKey(KeyCode.Mouse0)) _organSketch?.Record();
                _organSketch?.Update(Mathf.InverseLerp(1f, 12f, drawSpeedMulti));
            }
        }

        private bool _drawZone;
        public void DropDraw()
        {
            _anim.SetBool("Draw", false);
            _drawZone = false;
            cursorSpeed = _startCursorSpeed;
        }
        
        public void DropDrawZone()
        {
            if (!_isOpen || !_isMore) return;
            _anim.SetBool("Focused", false);
            _isMore = false;
            _z = cursor.position.z;
            TrailEmitting(false);
                
            if (!_isMore) OffsetTrail(false);
            else OffsetTrail(true);
        }
        
        public void EnterDraw()
        {
            if (!_isOpen || _isMore) return;
            _anim.SetBool("Focused", true);
            _isMore = true;
            _z = cursor.position.z;
            TrailEmitting(false);
            // Outline intensity follows drawing progress in Update.
                
            if (!_isMore) OffsetTrail(false);
            else OffsetTrail(true);
        }

        private float _kickTime = 2f;
        private bool _kick;

        private void FixedUpdate()
        {
            cursor.linearVelocity = new Vector3(_cursorDir.x, _cursorDir.y, 0f) * cursorSpeed;
            if (cursor.linearVelocity.sqrMagnitude < 0.0001f && Input.GetKey(KeyCode.Mouse0) && _drawZone)
            {
                if (_kickTime > 0f) _kickTime -= Time.deltaTime;
                else
                {
                    cursor.linearVelocity += new Vector3(1, 1, 0) * (Random.Range(-1f, 1f) * Random.Range(12f, 20f));
                    _kick = true;
                }
            }
            else _kickTime = 2f;
            if (_kick)
            {
                _kickTime = 2f * 2f;
                _kick = false;
            }
            
            var pos = cursor.position;

            var cam = Camera.main;
            if (cam == null) return;
            var min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.WorldToScreenPoint(pos).z));
            var max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.WorldToScreenPoint(pos).z));
            
            var tipOffset = drawDots.Length > 0 ? drawDots[0].transform.position - cursor.position : Vector3.zero;
            pos.x = Mathf.Clamp(pos.x, min.x - tipOffset.x, max.x - tipOffset.x);
            pos.y = Mathf.Clamp(pos.y, min.y - tipOffset.y, max.y - tipOffset.y);
            pos.z = _z;

            cursor.MovePosition(pos);
        }

        private void TrailEmitting(bool emit)
        {
            drawDots.ForEach(dot => dot.emitting = emit && _isOpen && !_completed
                && _organSketch == null);
        }

        private async void OffsetTrail(bool inversedOffset)
        {
            await UniTask.Delay(60);
            
            var offset = trailOffset;
            if (inversedOffset) offset = -offset;

            const float duration = 0.7f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                await UniTask.NextFrame();
                var step = Mathf.Min(Time.deltaTime, duration - elapsed);
                elapsed += step;
                _organSketch?.Offset(offset * (step / duration));
                foreach (var trail in drawDots)
                {
                    var count = trail.positionCount;
                    if (count == 0) continue;
                    var positions = new Vector3[count];
                    trail.GetPositions(positions);
                    for (var i = 0; i < count; i++)
                        positions[i] += offset * (step / duration);
                    trail.SetPositions(positions);
                }
            }
        }

        public async UniTask EnterDiary(System.Action<float> onDrawProgress = null,
            RabbitType rabbitType = RabbitType.Vision)
        {
            _onDrawProgress = onDrawProgress;
            _isMore = false;
            _anim.SetBool("Focused", false);
            _anim.SetTrigger("Enter");
            _isOpen = true;
            NarrativeOutlineDirector.StartRabbitSketch();
            OutlineMover.SetSketchProgress(0f);
            
            cursor.transform.localPosition = Vector3.zero;
            if (dotTimes.Length > 0)
            {
                for (var i = 0; i < drawDots.Length; i++)
                {
                    drawDots[i].time = dotTimes[i];
                }
            }
            else dotTimes = new float[drawDots.Length];
            _organSketch = new OrganSketch(drawDots, rabbitType, organSketchSize, transform.Find("left/notepad")?.GetComponent<SpriteRenderer>(), organInkMaterial, organSplatterMaterial);
            
            await UniTask.WaitUntil(() => _exit);
            _anim.SetBool("Focused", false);
            _anim.SetBool("Draw", false);
            _anim.SetTrigger("Exit");
            TrailEmitting(false);
            foreach (var dot in drawDots) dot.Clear();
            _organSketch.Restore();
            _organSketch = null;
            _kick = false;
            _isMore = false;
            _drawZone = false;
            drawSpeedMulti = 1f;
            cursorSpeed = _startCursorSpeed;
            
            _onDrawProgress = null;
            _completed = false;
            _isOpen = false;
            _exit = false;
            
            // The catch accent starts with Rip, rather than after the animation.
            await UniTask.Delay(150);
            NarrativeOutlineDirector.ReturnToCalm();
        }
    }
}
