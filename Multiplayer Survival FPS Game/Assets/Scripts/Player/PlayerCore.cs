using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HybridJK.MultiplayerSurvival.State;

namespace HybridJK.MultiplayerSurvival.Player
{
    public class PlayerCore : StateMachineMB
    {
        public IdleState IdleState { get; private set; }
        public WalkState WalkState { get; private set; }
        public SprintState SprintState { get; private set; }
        public CrouchState CrouchState { get; private set; }

        [Header("Private Variables")]
        private Inputs inputs;
        public InputAction movement;
        private InputAction sprint;
        private InputAction crouch;
        private InputAction jump;
        private Vector2 currentMouseDelta;
        private Vector2 currentMouseDeltaVelocity;
        private float cameraPitch;
        private float playerEyePitch;

        [Header("Public Variables")]
        public Vector2 directionVelocity = Vector2.zero;
        public Vector2 direction = Vector2.zero;
        public Vector3 velocity = Vector3.zero;
        public float velocityY = 0f;
        public bool isWalking;
        public bool isSprinting;
        public bool isCrouching;
        public string currentState;

        [Header("Required Referances")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CharacterController playerController;
        [SerializeField] private Transform playerEyes;

        [Header("Keyboard Movement Settings")]
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] public float airSpeed = 0.5f;
        [SerializeField] public float sideAirSpeed = 0.2f;
        [Range(0.01f, 0.5f)] public float movementSmoothTime = 0.3f;
        [SerializeField] public float jumpHeight = 1f;
        [SerializeField] public float jumpSmoothTime = 0.3f;

        [Header("Mouse Movement Settings")]
        [Range(0.1f, 2f)][SerializeField] private float mouseSensitivity = 0.7f;
        [Range(0f, 0.5f)][SerializeField] private float mouseSmoothTime = 0.03f;

        [Header("Gravity Settings")]
        [SerializeField] private float gravity = -13f;

        private void Awake()
        {
            //------ ALL CLIENTS RUN CODE BELOW ------//
            //Initialize Inputs
            inputs = new Inputs();
            //Initialize States
            IdleState = new IdleState(this, transform, playerController);
            WalkState = new WalkState(this, transform, playerController, walkSpeed);
            SprintState = new SprintState(this, transform, playerController, sprintSpeed, walkSpeed);
            CrouchState = new CrouchState(this, transform, playerController, crouchSpeed);
        }
        private void OnEnable()
        {
            //------ ALL CLIENTS RUN CODE BELOW ------//
            //InputActions Initialization
            movement = inputs.Player.Movement;
            sprint = inputs.Player.Sprint;
            crouch = inputs.Player.Crouch;
            jump = inputs.Player.Jump;
            //Subscribing Methods
            sprint.performed += SprintToggled;
            crouch.performed += CrouchToggled;
            movement.started += MovementStarted;
            movement.canceled += MovementStopped;
            jump.performed += PlayerJump;
            //Enable Inputs
            movement.Enable();
            sprint.Enable();
            crouch.Enable();
            jump.Enable();
        }
        private void OnDisable()
        {
            if (isLocalPlayer)
            {
                //------ ALL CODE BELOW IS LOCAL ONLY ------//
                //Unsubscribing Methods
                sprint.performed -= SprintToggled;
                crouch.performed -= CrouchToggled;
                movement.started -= MovementStarted;
                movement.canceled -= MovementStopped;
                jump.performed -= PlayerJump;
                //InputActions Disabling
                movement.Disable();
                sprint.Disable();
                crouch.Disable();
            }
        }
        private void Start()
        {
            if (isLocalPlayer)
            {
                //------ ALL CODE BELOW IS LOCAL ONLY ------//
                ChangeState(IdleState);
                Debug.Log("Starting State Machine, Initial State: IdleState");
                Cursor.lockState = CursorLockMode.Locked;
            }
            //------ ALL CLIENTS RUN CODE BELOW ------//
        }
        public void MouseMovement()
        {
            Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); //Target position for the mouse
            currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime); //Smoothing the Mouse Movement from current pos to target pos with speed
            transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity); //rotating the y rotation of camera with the body in the upward axis
            cameraPitch -= currentMouseDelta.y * mouseSensitivity; //Inverting and creating the camera x-axis rotation
            cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f); //Constraining the camera x-axis rotation to straight up & down
            playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch; //Rotating the local rotation of the camera on the x-axis
            playerEyePitch = Mathf.Clamp(cameraPitch, -30f, 30f); //Constraining the player eye's x-rotation to -30 - 30 degrees
            playerEyes.transform.localEulerAngles = Vector3.right * playerEyePitch; //Rotating the local rotation of the player eye's on the x-axis (Player eye rotation and camera rotation will match, only difference is that eye's can't go straight up and down)
        }
        public void ApplyGravity()
        {
            if (playerController.isGrounded)
            {
                velocityY = gravity * Time.deltaTime;
            }
            else
            {
                velocityY += gravity * Time.deltaTime;
            }
        }
        private void SprintToggled(InputAction.CallbackContext obj)
        {
            if (isLocalPlayer)
            {
                //------ ALL CODE BELOW IS LOCAL ONLY ------//
                if (isWalking)
                {
                    isSprinting = !isSprinting;
                }
            }
        }
        private void CrouchToggled(InputAction.CallbackContext obj)
        {
            if (isLocalPlayer)
            {
                //------ ALL CODE BELOW IS LOCAL ONLY ------//
                isCrouching = !isCrouching;
            }
        }
        private void MovementStarted(InputAction.CallbackContext obj)
        {
            if (isLocalPlayer)
            {
                //------ ALL CODE BELOW IS LOCAL ONLY ------//
                isWalking = !isWalking;
            }
        }
        private void MovementStopped(InputAction.CallbackContext obj)
        {
            if (isLocalPlayer)
            {
                //------ ALL CODE BELOW IS LOCAL ONLY ------//
                isWalking = !isWalking;
            }
        }
        private void PlayerJump(InputAction.CallbackContext obj)
        {
            //Testing other stuff
        }
        public IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
    }
}