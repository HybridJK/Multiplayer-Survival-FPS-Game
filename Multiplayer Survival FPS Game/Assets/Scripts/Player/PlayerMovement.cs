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
        private InputAction sprint;
        private InputAction crouch;

        //Keyboard Movement Variables
        private Vector2 currentDir = Vector2.zero;
        private Vector2 currentDirVelocity = Vector2.zero;
        private bool isSprinting = false;
        private bool isCrouching = false;
        private bool startCrouchAnimation = false;
        private bool crouchEnumeratorRunning = false;
        private bool hasTurnedInAir = false;

        //Mouse Movement Variables
        private Vector2 targetMouseDelta = Vector2.zero;
        private float cameraPitch = 0f;
        private Vector2 currentMouseDelta = Vector2.zero;
        private Vector2 currentMouseDeltaVelocity = Vector2.zero;

        //Gravity Variables
        private float velocityY = 0f;

        //Jump Variables
        private bool isJumping = false;

        [Header("Referances")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private Camera playerCamera;

        [Header("Keyboard Movement Settings")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float sprintSpeed = 4f;
        [SerializeField] private float sideSprintSpeed = 3f;
        [SerializeField] private float groundTurnSpeed = 2.5f;
        [SerializeField] private float airMovementSpeed = 2.5f;
        [Range(0f, 0.5f)][SerializeField] private float movementSmoothTime = 2f;
        [SerializeField] private float JumpForce = 1f;

        [Header("Crouch Settings")]
        [SerializeField] private float crouchMovementSpeed = 1f;
        [Range(0f, 2f)] [SerializeField] private float crouchAnimationSpeed = 0.5f;
        [SerializeField] private Vector3 startCameraCrouchPos;
        [SerializeField] private Vector3 endCameraCrouchPos;

        [Header("Mouse Movement Settings")]
        [Range(0.1f, 2f)]
        [SerializeField] private float mouseSensitivity = 5f;
        [Range(0f, 0.5f)]
        [SerializeField] private float mouseSmoothTime = 0.03f;
        [Range(0.01f, 0.5f)] [SerializeField] private float mouseAngleSprintCutOff = 0.2f;

        [Header("Gravity Settings")]
        [SerializeField] private float gravity = -13f;

        [Header("Slope Settings")]
        [SerializeField] private float slopeForce;
        [SerializeField] private float slopeForceRayLength;

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
            sprint = inputs.Player.Sprint;
            crouch = inputs.Player.Crouch;
            sprint.performed += SprintToggled;
            crouch.performed += CrouchToggled;
            movement.Enable();
            sprint.Enable();
            crouch.Enable();
        }
        private void OnDisable()
        {
            movement.Disable();
            sprint.Disable();
            crouch.Disable();
        }
        private void Update()
        {
            // ------ Player Movement (Keyboard) ------
            KeyboardMovement();

            // ------ Crouch Amination ------
            if (startCrouchAnimation) //If the crouch animation needs to be ran
            {
                if (crouchEnumeratorRunning) //If the crouch animation is already running
                {
                    StopAllCoroutines(); //Stop current crouch animation
                    crouchEnumeratorRunning = false; //Update crouch animation running variable
                }
                StartCoroutine(SmoothCrouchAnimation(crouchAnimationSpeed)); //Start new crouch animation
                crouchEnumeratorRunning = true; //Update crouch animation running variable
                startCrouchAnimation = false; //Update crouch animation needing to start variable
            }
        }
        private void LateUpdate()
        {
            // ------ Mouse Movement -------
            MouseMovement();
        }
        private void KeyboardMovement()
        {
            // ------ Movement Calculatons ------
            Vector2 targetDir = new Vector2(movement.ReadValue<Vector2>().x, movement.ReadValue<Vector2>().y); //Target values for movement
            if ((targetDir == Vector2.zero || targetDir.y < 0f) && isSprinting) //If player not moving or player is moving backwards and is sprinting
            {
                isSprinting = false; //Disable sprint
            }
            currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, movementSmoothTime); //Smoothing the movement from the current pos to the target pos with time

            // ------ Slope Calculations
            if ((targetDir.x != 0 || targetDir.y != 0) && OnSlope()) //If player is moving and is on a slope
            {
                controller.Move(Vector3.down * controller.height / 2 * slopeForce * Time.deltaTime); //Apply greater downwards force
            }

            // ------ Jumping Calculations ------
            if (controller.isGrounded) //Checking if the character is on the ground
            {
                velocityY = gravity * Time.deltaTime; //If player is on the ground - Set to gravity
                if (Input.GetKey(KeyCode.Space)) //If player is on the ground & pressing Jump - Jump
                {
                    velocityY = JumpForce;
                }
            }
            else
            {
                velocityY += gravity * Time.deltaTime; //If player is not on the ground - Apply gravity
            }

            // ------ Final Calculations & Applying forces to character
            Vector3 velocity = ((transform.forward * currentDir.y) * GetCurrentMovementSpeed(true)) + ((transform.right * currentDir.x) * GetCurrentMovementSpeed(false)) + Vector3.up * velocityY; //Move player forward depending on current y value multiplied by forward speed, move player left-right depending on current x value multiplied by side speed, add gravity at the end
            controller.Move(velocity * Time.deltaTime); //Moving the controller with velocity and time.deltaTime
        } //Does all the player movement logic
        private void MouseMovement()
        {
            targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); //Target position for the mouse
            currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime); //Smoothing the Mouse Movement from current pos to target pos with speed
            transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity); //rotating the y rotation of camera with the body in the upward axis
            cameraPitch -= currentMouseDelta.y * mouseSensitivity; //Inverting and creating the camera x-axis rotation
            cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f); //Constraining the camera x-axis rotation to straight up & down
            playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch; //Rotating the local rotation of the camera on the x-axis
        } //Does all the mouse movement logic
        private bool OnSlope() 
        {
            if (isJumping) { return false; } //If jumping - Not on slope

            RaycastHit hit; //Holds raycast hit value
            if (Physics.Raycast(transform.position, Vector3.down, out hit, controller.height / 2 * slopeForceRayLength)) //Create raycast from player center down into the floor
            {
                if (hit.normal != Vector3.up) //If raycast is not hitting a flat surface (not a slope)
                {
                    return true; //Return is on slope
                }
            }
            return false; //Return is not on slope
        } //Check if the player is on a slope
        private float GetCurrentMovementSpeed(bool forwardMovement)
        {
            if (forwardMovement && isSprinting) //If requesting forward speed & is sprinting
            {
                if (controller.isGrounded && IsTurningFast()) //If the controller is grounded and is turning fast
                {
                    if (hasTurnedInAir) { hasTurnedInAir = false; } //Update if the player has turned fast while in the air
                    return groundTurnSpeed; //Return a speed while the player is sprinting and turning fast
                }
                else if (!controller.isGrounded && (IsTurningFast() || hasTurnedInAir)) //If the player is in the air & is turning fast or has already turned fast in the air
                {
                    hasTurnedInAir = true; //Update if the player has turned fast in the air
                    return airMovementSpeed; //Return a speed while the player is sprinting and turning fast in the air
                }
                if (hasTurnedInAir) { hasTurnedInAir = false; } //Update if player has turned fast in the air
                return sprintSpeed; //If player is sprinting, is not in the air, and is not turning fast, return a speed of player sprinting
            }
            else if (isCrouching) //If the player is crouching
            {
                return crouchMovementSpeed; //Return the player crouch speed
            }
            else //If the player is not sprinting forward or crouching
            {
                if (isSprinting) //If the player is sprinting but not requesting forward speed
                {
                    return sideSprintSpeed; //Give a speed of sprinting to the side
                }
                else if (!controller.isGrounded && (IsTurningFast() || hasTurnedInAir)) //If the player is in the air & turning fast or has turned in the air
                {
                    hasTurnedInAir = true; //Update has turned in the air variable
                    return airMovementSpeed; //Give a speed of moving in the air after turning
                }
                if (hasTurnedInAir) { hasTurnedInAir = false; } //If not sprinting sideways, or turning fast in the air, then update has turned in air speed
                return walkSpeed; //If not sprinting sideways, or turning fast in the air, then give a walking speed
            }
        } //Return the speed of the player
        private void SprintToggled(InputAction.CallbackContext obj)
        {
            if (isCrouching) //If is crouching
            {
                isCrouching = false; //Diable crouch
                isSprinting = true; //Enable sprint
                startCrouchAnimation = true; //Start crouch animation
            }
            else //If is not sprinting
            {
                isSprinting = !isSprinting; //Disable Sprint
            }
        } //Toggle sprint
        private void CrouchToggled(InputAction.CallbackContext obj)
        {
            if (isSprinting) //If is sprinting
            {
                isSprinting = false; //Disable sprint
                isCrouching = true; //Enable crouch
            }
            else //If is not sprinting
            {
                isCrouching = !isCrouching; //Toggle crouch
            }
            startCrouchAnimation = true; //Start crouch animation
        } //Toggle crouch
        private IEnumerator SmoothCrouchAnimation(float smoothTime)
        {
            Vector3 endPos = Vector3.zero;
            Vector3 startPos = playerCamera.transform.localPosition;
            if (!isCrouching) //If is uncrouching
            {
                endPos = startCameraCrouchPos;
            }
            else //If is crouching
            {
                endPos = endCameraCrouchPos;
            }

            float elapsedTime = 0f;
            while (elapsedTime <= smoothTime) //While the crouchAnimation time has not passed
            {
                playerCamera.transform.localPosition = Vector3.Lerp(startPos, endPos, (elapsedTime/smoothTime)); //Get next smoothed crouch camera position
                elapsedTime += Time.deltaTime; //Increase the time elapsed
                if (elapsedTime >= smoothTime) //If the animation should be ending
                {
                    crouchEnumeratorRunning = false; //Update animation running variable
                }
                yield return null;
            }
        } //Does the crouch animation
        private bool IsTurningFast()
        {
            if (targetMouseDelta.x >= mouseAngleSprintCutOff || targetMouseDelta.x <= -mouseAngleSprintCutOff) //If the mouse if moving faster then a set amount
            {
                return true; //Then return that the player is turning fast
            }
            return false;
        } //Returns if the player is turning fast
    }

}