using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

namespace HybridJK.MultiplayerSurvival.PlayerMovement
{
    public class PlayerMovement : NetworkBehaviour
    {
        private Inputs inputs;
        private InputAction movement;
        private Vector3 move = Vector3.zero;
        private float xRotation = 0f;

        [SerializeField] private CharacterController controller;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float movementSpeed = 2f;
        [SerializeField] private float JumpHeight = 1f;
        [SerializeField] private float mouseSensitivity = 5f;

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

            // ------ Camera Movement (Mouse) ------
            MouseMovement();
        }
        private void KeyboardMovement()
        {
            float x = movement.ReadValue<Vector2>().x;
            float z = movement.ReadValue<Vector2>().y;
            move = transform.right * x + transform.forward * z; //Defining Movement Amound and Direction
            controller.Move(move * movementSpeed * Time.deltaTime);
        }
        private void MouseMovement()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); //Clamping Rotation to not over rotate or under rotate
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); //Rotate Camera up and down
            transform.Rotate(Vector3.up * mouseX); //Rotate player left and right
        }
    }

}