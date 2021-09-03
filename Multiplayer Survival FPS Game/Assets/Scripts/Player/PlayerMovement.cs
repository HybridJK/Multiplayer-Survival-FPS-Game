using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

namespace HybridJK.MultiplayerSurvival.PlayerMovement
{
    public class PlayerMovement : NetworkBehaviour
    {
        //Input Referances
        private Inputs inputs;
        private InputAction movement;

        //Keyboard Movement Variables
        private Vector2 currentDir = Vector2.zero;
        private Vector2 currentDirVelocity = Vector2.zero;

        //Mouse Movement Variables
        private float cameraPitch = 0f;
        private Vector2 currentMouseDelta = Vector2.zero;
        private Vector2 currentMouseDeltaVelocity = Vector2.zero;

        //Gravity Variables
        private float velocityY = 0f;

        [Header("Referances")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private Camera playerCamera;

        [Header("Keyboard Movement Settings")]
        [SerializeField] private float movementSpeed = 2f;
        [Range(0f, 0.5f)]
        [SerializeField] private float movementSmoothTime = 2f;
        [SerializeField] private float JumpHeight = 1f;

        [Header("Mouse Movement Settings")]
        [Range(0.1f, 2f)]
        [SerializeField] private float mouseSensitivity = 5f;
        [Range(0f, 0.5f)]
        [SerializeField] private float mouseSmoothTime = 0.03f;

        [Header("Gravity Settings")]
        [SerializeField] private float gravity = -13f;

        private void Awake()
        {
            inputs = new Inputs();
        }
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        private void OnEnable()
        {
            movement = inputs.Player.Movement;
            movement.Enable();

            inputs.Player.Jump.performed += Jump;
            inputs.Player.Jump.Enable();
        }
        private void OnDisable()
        {
            movement.Disable();
            inputs.Player.Jump.Disable();
        }
        private void Jump(InputAction.CallbackContext obj)
        {
            Debug.Log("Jump!");
        }
        private void Update()
        {
            // ------ Player Movement (Keyboard) ------
            KeyboardMovement();
        }
        private void LateUpdate()
        {
            // ------ Mouse Movement -------
            MouseMovement();
        }
        private void KeyboardMovement()
        {
            Vector2 targetDir = new Vector2(movement.ReadValue<Vector2>().x, movement.ReadValue<Vector2>().y); //Target values for movement
            currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, movementSmoothTime); //Smoothing the movement from the current pos to the target pos with time

            if (controller.isGrounded)
            {
                velocityY = 0f;
            }

            velocityY += gravity * Time.deltaTime;

            Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * movementSpeed + Vector3.up * velocityY; //create velocity by multiplying the forward with y direction, the right with x direction, and then adding together; Also adding the gravity at the end with forward but in negative direction
            controller.Move(velocity * Time.deltaTime); //Moving the controller with velocity and time.deltaTime
        }
        private void MouseMovement()
        {
            Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); //Target position for the mouse
            currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime); //Smoothing the Mouse Movement from current pos to target pos with speed
            transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity); //rotating the y rotation of camera with the body in the upward axis
            cameraPitch -= currentMouseDelta.y * mouseSensitivity; //Inverting and creating the camera x-axis rotation
            cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f); //Constraining the camera x-axis rotation to straight up & down
            playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch; //Rotating the local rotation of the camera on the x-axis
        }
    }

}