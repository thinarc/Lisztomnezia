using UnityEngine;

namespace EvolveGames
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float Velocity => characterController.velocity.magnitude;
        public float MaxSpeed => isRunning ? RuningSpeed : walkingSpeed;
        public bool IsRunning => isRunning;
        public bool targetView;
        
        [Header("PlayerController")]
        [SerializeField] public Transform Camera;
        [SerializeField] public Transform HCamera;
        [SerializeField] public ItemChange Items;
        [SerializeField, Range(1, 10)] float walkingSpeed = 3.0f;
        [Range(0.1f, 5)] public float CroughSpeed = 1.0f;
        [SerializeField, Range(2, 20)] float RuningSpeed = 4.0f;
        [SerializeField, Range(0, 20)] float jumpSpeed = 6.0f;
        [SerializeField, Range(0.01f, 10)] float lookSpeed = 2.0f;
        [SerializeField, Range(0, 120)] float lookXLimit = 12f;
        [SerializeField, Range(0, 120)] float lookYLimit = 16f;
        [Space(20)]
        [Header("Advance")]
        [SerializeField] float RunningFOV = 65.0f;
        [SerializeField] float SpeedToFOV = 4.0f;
        [SerializeField] float CroughHeight = 1.0f;
        [SerializeField] float gravity = 20.0f;
        [SerializeField] float timeToRunning = 2.0f;
        [HideInInspector] public bool canMove = true;
        [HideInInspector] public bool CanRunning = false;

        [Space(20)]
        [Header("Climbing")]
        [SerializeField] bool CanClimbing = true;
        [SerializeField, Range(1, 25)] float Speed = 2f;
        bool isClimbing = false;

        [Space(20)]
        [Header("HandsHide")]
        [SerializeField] bool CanHideDistanceWall = true;
        [SerializeField, Range(0.1f, 5)] float HideDistance = 1.5f;
        [SerializeField] int LayerMaskInt = 1;

        [Space(20)]
        [Header("Input")]
        [SerializeField] KeyCode CroughKey = KeyCode.LeftControl;


        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public Vector3 moveDirection = Vector3.zero;
        bool isCrough = false;
        float InstallCroughHeight;
        float rotationX = 0;
        float rotationY = 0;
        [HideInInspector] public bool isRunning = false;
        Vector3 InstallCameraMovement;
        float InstallFOV;
        Camera cam;
        [HideInInspector] public bool Moving;
        [HideInInspector] public float vertical;
        [HideInInspector] public float horizontal;
        [HideInInspector] public float Lookvertical;
        [HideInInspector] public float Lookhorizontal;
        float RunningValue;
        float installGravity;
        bool WallDistance;
        [HideInInspector] public float WalkingValue;
        void Start()
        {
            characterController = GetComponent<CharacterController>();
            if (Items == null && GetComponent<ItemChange>()) Items = GetComponent<ItemChange>();
            cam = GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InstallCroughHeight = characterController.height;
            InstallCameraMovement = Camera.localPosition;
            InstallFOV = cam.fieldOfView;
            RunningValue = RuningSpeed;
            installGravity = gravity;
            WalkingValue = walkingSpeed;
        }
        
        public float edgeSlowZone = 20f;

        void Update()
        {
            RaycastHit CroughCheck;
            RaycastHit ObjectCheck;

            if (!characterController.isGrounded && !isClimbing)
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            // isRunning = !isCrough ? CanRunning ? Input.GetKey(KeyCode.LeftShift) : false : false;
            isRunning = !isCrough ? CanRunning ? false : false : false;
            vertical = canMove ? (isRunning ? RunningValue : WalkingValue) * Input.GetAxis("Vertical") : 0;
            if (vertical < 0) vertical = 0;
            // if (vertical < 0) vertical = Random.Range(vertical, vertical * 16f) / 40f;
            // horizontal = canMove ? (isRunning ? RunningValue : WalkingValue) * Input.GetAxis("Horizontal") : 0;
            horizontal = 0;
            if (isRunning) RunningValue = Mathf.Lerp(RunningValue, RuningSpeed, timeToRunning * Time.deltaTime);
            else RunningValue = WalkingValue;
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * vertical) + (right * horizontal);

            if (Input.GetButton("Jump") && canMove && characterController.isGrounded && !isClimbing)
            {
                moveDirection.y = jumpSpeed;
            }
            else
            {
                moveDirection.y = movementDirectionY;
            }
            characterController.Move(moveDirection * Time.deltaTime);
            Moving = horizontal < 0 || vertical < 0 || horizontal > 0 || vertical > 0 ? true : false;

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = -Input.GetAxis("Mouse Y");
                
                float deltaX = mouseY * lookSpeed;
                float nextX = rotationX + deltaX;

                float distX = lookXLimit - Mathf.Abs(rotationX);
                float factorX = Mathf.Clamp01(distX / edgeSlowZone);

// движемся ли к границе
                bool goingToLimitX = Mathf.Abs(nextX) > Mathf.Abs(rotationX);

                float finalFactorX = goingToLimitX ? factorX : 1f;

                if (!targetView) rotationX += deltaX * finalFactorX;
                else
                {
                    rotationX += (deltaX * 0.4f) * finalFactorX; // 8.23f
                    if (Mathf.Abs(rotationX) - 8.23f > 0.5f) rotationX = Mathf.MoveTowards(rotationX, 8.23f, Time.deltaTime * Random.Range(32, 36) * 4f);
                    else rotationX = Mathf.MoveTowards(rotationX, Random.Range(7.4f, 8.7f), Time.deltaTime * 24);
                }
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
                
                float deltaY = mouseX * lookSpeed;
                float nextY = rotationY + deltaY;

                float distY = lookYLimit - Mathf.Abs(rotationY);
                float factorY = Mathf.Clamp01(distY / edgeSlowZone);

                bool goingToLimitY = Mathf.Abs(nextY) > Mathf.Abs(rotationY);

                float finalFactorY = goingToLimitY ? factorY : 1f;

                if (!targetView) rotationY += deltaY * finalFactorY;
                else
                {
                    rotationY += (deltaY * 0.4f) * finalFactorY; // -2.24f
                    if (Mathf.Abs(rotationY) - 2.24f > 0.5f) rotationY = Mathf.MoveTowards(rotationY, -2.24f, Time.deltaTime * Random.Range(24, 28) * 2f);
                    else rotationY = Mathf.MoveTowards(rotationY, Random.Range(-2.9f, -1.6f), Time.deltaTime * 24);
                }
                rotationY = Mathf.Clamp(rotationY, -lookYLimit, lookYLimit);
                
                Camera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                HCamera.transform.localRotation = Quaternion.Euler(0, rotationY, 0);

                if (isRunning && Moving) cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, RunningFOV, SpeedToFOV * Time.deltaTime);
                else cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, InstallFOV, SpeedToFOV * Time.deltaTime);
            }

            if (Input.GetKey(CroughKey))
            {
                isCrough = true;
                float Height = Mathf.Lerp(characterController.height, CroughHeight, 5 * Time.deltaTime);
                characterController.height = Height;
                WalkingValue = Mathf.Lerp(WalkingValue, CroughSpeed, 6 * Time.deltaTime);

            }
            else if (!Physics.Raycast(GetComponentInChildren<Camera>().transform.position, transform.TransformDirection(Vector3.up), out CroughCheck, 0.8f, 1))
            {
                if (characterController.height != InstallCroughHeight)
                {
                    isCrough = false;
                    float Height = Mathf.Lerp(characterController.height, InstallCroughHeight, 6 * Time.deltaTime);
                    characterController.height = Height;
                    WalkingValue = Mathf.Lerp(WalkingValue, walkingSpeed, 4 * Time.deltaTime);
                }
            }

            if(WallDistance != Physics.Raycast(GetComponentInChildren<Camera>().transform.position, transform.TransformDirection(Vector3.forward), out ObjectCheck, HideDistance, LayerMaskInt) && CanHideDistanceWall)
            {
                WallDistance = Physics.Raycast(GetComponentInChildren<Camera>().transform.position, transform.TransformDirection(Vector3.forward), out ObjectCheck, HideDistance, LayerMaskInt);
                Items.ani.SetBool("Hide", WallDistance);
                Items.DefiniteHide = WallDistance;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Ladder" && CanClimbing)
            { 
                CanRunning = false;
                isClimbing = true;
                WalkingValue /= 2;
                Items.Hide(true);
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.tag == "Ladder" && CanClimbing)
            {
                moveDirection = new Vector3(0, Input.GetAxis("Vertical") * Speed * (-Camera.localRotation.x / 1.7f), 0);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Ladder" && CanClimbing)
            {
                CanRunning = true;
                isClimbing = false;
                WalkingValue *= 2;
                Items.ani.SetBool("Hide", false);
                Items.Hide(false);
            }
        }

    }
}