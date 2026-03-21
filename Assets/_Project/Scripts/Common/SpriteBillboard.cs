using UnityEngine;

namespace _Project.Scripts.Render
{
    [ExecuteAlways]
    public class SpriteBillboard : MonoBehaviour
    {
        private Camera _cam;

        private void Start()
        {
            if (_cam == null && Camera.main != null) _cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (_cam == null) return;
            
            var direction = _cam.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-direction);

            // Если нужно, чтобы спрайт оставался вертикально ориентированным (без наклона)
            //Vector3 euler = transform.rotation.eulerAngles;
            //transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
        }
    }
}