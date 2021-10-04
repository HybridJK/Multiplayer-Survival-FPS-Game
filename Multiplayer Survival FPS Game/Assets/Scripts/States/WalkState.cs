using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HybridJK.MultiplayerSurvival.Player;

namespace HybridJK.MultiplayerSurvival.State
{
    public class WalkState : IState
    {
        private PlayerCore playerCore;
        private Transform player;
        private CharacterController playerController;
        private float movementSpeed;
        private bool hasJumped;

        public WalkState(PlayerCore playerCore, Transform player, CharacterController playerController, float movementSpeed)
        {
            this.playerCore = playerCore;
            this.player = player;
            this.playerController = playerController;
            this.movementSpeed = movementSpeed;
        }
        public void Enter()
        {
            //TODO - Gradually increase speed until walk speed, then stay constant walk speed
        }
        public void Exit()
        {
        }
        public void FixedTick()
        {
            playerCore.currentState = playerCore.CurrentState.ToString();
        }
        public void Tick()
        {
            ChangeStateBehaviour();
            playerCore.ApplyGravity();
            WalkStateMovement();
        }
        public void LateTick()
        {
            playerCore.MouseMovement();
        }
        private void ChangeStateBehaviour() //Handles the changing state behavious for WalkState 
        {
            //While in WalkState
            if (playerCore.isCrouching) //Starts crouching
            {
                playerCore.ChangeState(playerCore.CrouchState); //Change to CrouchState
            }
            else if (!playerCore.isWalking) //Stops walking
            {
                playerCore.ChangeState(playerCore.IdleState); //Change to IdleState
            }
            else if (playerCore.isSprinting) //Starts sprinting
            {
                playerCore.ChangeState(playerCore.SprintState); //Change to SprintState
            }
        }
        private void WalkStateMovement() //Calculates the next velocity while walking 
        {
            //Movement Calculations
            if (Input.GetKey(KeyCode.Space) && playerController.isGrounded)
            {
                playerCore.velocityY = playerCore.jumpHeight;
            }
            if (!playerController.isGrounded)
            {
                Vector2 targetPos = new Vector2(playerCore.movement.ReadValue<Vector2>().x, playerCore.movement.ReadValue<Vector2>().y);
                playerCore.direction = Vector2.SmoothDamp(playerCore.direction, targetPos, ref playerCore.directionVelocity, playerCore.movementSmoothTime);
                playerCore.velocity += (player.transform.forward * playerCore.direction.y * playerCore.airSpeed) + (player.transform.right * playerCore.direction.x * playerCore.sideAirSpeed);
                playerCore.velocity.y = Mathf.Lerp(playerCore.velocity.y, playerCore.velocityY, playerCore.jumpSmoothTime);
            }
            else
            {
                Vector2 targetPos = new Vector2(playerCore.movement.ReadValue<Vector2>().x, playerCore.movement.ReadValue<Vector2>().y);
                playerCore.direction = Vector2.SmoothDamp(playerCore.direction, targetPos, ref playerCore.directionVelocity, playerCore.movementSmoothTime);
                playerCore.velocity = ((player.transform.forward * playerCore.direction.y + player.transform.right * playerCore.direction.x) * movementSpeed) + (Vector3.up * playerCore.velocityY);
            }

            //Apply Movement
            playerController.Move(playerCore.velocity * Time.deltaTime);
        }
    }
}
