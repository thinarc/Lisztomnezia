using UnityEngine;

namespace _Project.Scripts.osu_.Align
{
    public class HitPositioner : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private RectTransform playfield; // RectTransform канваса или панели

        [Header("Distance")]
        [SerializeField] private float minDistance = 120f;
        [SerializeField] private float maxDistance = 300f;

        [Header("Direction Control")]
        [SerializeField] private float directionRandomness = 0.5f;

        [Header("Bounds Padding")]
        [SerializeField] private float padding = 100f;

        private Vector2 lastPosition;
        private Vector2 lastDirection = Vector2.right;

        private bool initialized;

        [SerializeField, Header("Pos Difficult")] private float convergence = 1f;
        private float _calledDelay;

        // Вызывать при первом спавне
        public void Initialize()
        {
            Rect rect = playfield.rect;

            lastPosition = new Vector2(
                Random.Range(rect.xMin + padding, rect.xMax - padding),
                Random.Range(rect.yMin + padding, rect.yMax - padding)
            );

            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;
            _calledDelay -= Time.deltaTime;
        }

        public Vector3 GetNextPosition(bool getRot)
        {
            if (!initialized)
                Initialize();
            
            var rect = playfield.rect;
            var rot = 0f;

            for (int i = 0; i < 10; i++)
            {
                // Направление с небольшим шумом
                Vector2 dir = (lastDirection + Random.insideUnitCircle * directionRandomness).normalized;
                float distance = Random.Range(minDistance, maxDistance);
                if (_calledDelay > 0)
                {
                    var divider = 1 + _calledDelay / convergence;
                    distance /= divider;

                    if (getRot) rot = 360f * _calledDelay / convergence;
                }

                Vector2 candidate = lastPosition + dir * distance;

                if (IsInside(candidate, rect))
                {
                    lastDirection = dir;
                    lastPosition = candidate;
                    return candidate;
                }
                Initialize();
            }

            // fallback — clamp в границы
            Vector2 clamped = ClampToRect(lastPosition, rect);
            lastPosition = clamped;
            _calledDelay = convergence;
            return new Vector3(clamped.x, clamped.y, rot);
        }

        private bool IsInside(Vector2 pos, Rect rect)
        {
            return pos.x > rect.xMin + padding &&
                   pos.x < rect.xMax - padding &&
                   pos.y > rect.yMin + padding &&
                   pos.y < rect.yMax - padding;
        }

        private Vector2 ClampToRect(Vector2 pos, Rect rect)
        {
            return new Vector2(
                Mathf.Clamp(pos.x, rect.xMin + padding, rect.xMax - padding),
                Mathf.Clamp(pos.y, rect.yMin + padding, rect.yMax - padding)
            );
        }
    }
}