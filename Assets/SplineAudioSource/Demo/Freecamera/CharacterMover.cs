using UnityEngine;

namespace SplineAS_DEMO
{
    public class CharacterMover : MonoBehaviour
    {
        public float moveVelocity = 3f;
        public float lookVelocity = 3f;

        public Transform cameraTransform;

        private float verticalRotation = 0f;

        void Update()
        {
            Vector3 moveAxis = new Vector3(
                Input.GetAxisRaw("Horizontal"),
                0,
                Input.GetAxisRaw("Vertical")
            );

            Vector3 moveDirection = (moveAxis.x * transform.right + moveAxis.z * cameraTransform.forward).normalized;

            if (Input.GetKey(KeyCode.LeftShift))
                moveDirection *= 5;

            transform.position += moveDirection * (moveVelocity * Time.deltaTime);
            
            float mouseX = Input.GetAxis("Mouse X") * lookVelocity;
            transform.Rotate(Vector3.up * mouseX);

            float mouseY = Input.GetAxis("Mouse Y") * lookVelocity;
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
}