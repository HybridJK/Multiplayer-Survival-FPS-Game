using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HybridJK.MultiplayerSurvival.Player;

namespace HybridJK.MultiplayerSurvival.State
{
    public class IdleState : IState
    {
        private PlayerCore playerCore;
        private Transform player;
        private CharacterController playerController;
        public IdleState(PlayerCore playerCore, Transform player, CharacterController playerController)
        {
            this.playerCore = playerCore;
            this.player = player;
            this.playerController = playerController;
        }
        public void Enter()
        {
        }
        public void Exit()
        {
            playerCore.velocity = Vector2.zero;
        }
        public void FixedTick()
        {
            playerCore.currentState = playerCore.CurrentState.ToString();
        }
        public void Tick()
        {
            ChangeStateBehaviour();
            playerCore.ApplyGravity();
            IdleStateMovement();
        }
        public void LateTick()
        {
            playerCore.MouseMovement();
        }
        private void ChangeStateBehaviour() //Handles the changing state behaviour for IdleState 
        {
            //While in IdleState
            if (playerCore.isWalking) //Starts walking
            {
                playerCore.ChangeState(playerCore.WalkState); //Change to WalkState
            }
            if (playerCore.isCrouching) //Starts crouching
            {
                playerCore.ChangeState(playerCore.CrouchState); //Change to CrouchState
            }
        }
        private void IdleStateMovement() //Stop the player movement smoothly 
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
                playerCore.direction = Vector2.SmoothDamp(playerCore.direction, Vector2.zero, ref playerCore.directionVelocity, playerCore.movementSmoothTime);
                playerCore.velocity = player.transform.forward * playerCore.direction.y + player.transform.right * playerCore.direction.x + (Vector3.up * playerCore.velocityY);
            }

            //Apply Movement
            playerController.Move(playerCore.velocity * Time.deltaTime);
        }
    }
}
