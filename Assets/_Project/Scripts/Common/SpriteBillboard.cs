using UnityEngine;

namespace _Project.Scripts.Render
{
    [ExecuteAlways]
    public class SpriteBillboard : MonoBehaviour
    {
        private const float MaxUpdateDistance = 10f;

        private Camera _cam;
        private float _sqrMaxDistance;

        private void Awake()
        {
            CacheCamera();
            _sqrMaxDistance = MaxUpdateDistance * MaxUpdateDistance;
        }

        private void OnValidate()
        {
            _sqrMaxDistance = MaxUpdateDistance * MaxUpdateDistance;
        }

        private void CacheCamera()
        {
            if (_cam == null && Camera.main != null)
                _cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (_cam == null)
            {
                CacheCamera();
                if (_cam == null) return;
            }

            Vector3 camPos = _cam.transform.position;
            Vector3 objPos = transform.position;
            float sqrDistance = (camPos - objPos).sqrMagnitude;
            if (sqrDistance > _sqrMaxDistance)
                return;

            Vector3 direction = camPos - objPos;
            transform.rotation = Quaternion.LookRotation(-direction);

            // Ограничение по Y (если нужно)
            // Vector3 euler = transform.rotation.eulerAngles;
            // transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
        }
    }
}