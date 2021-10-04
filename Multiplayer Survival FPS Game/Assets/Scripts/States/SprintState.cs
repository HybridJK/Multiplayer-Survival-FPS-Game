using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJK.MultiplayerSurvival.Player;

namespace HybridJK.MultiplayerSurvival.State
{
    public class SprintState : IState
    {
        private PlayerCore playerCore;
        private Transform player;
        private CharacterController playerController;
        private float forwardMovementSpeed;
        private float sideMovementSpeed;

        public SprintState(PlayerCore playerCore, Transform player, CharacterController playerController, float forwardMovementSpeed, float sideMovementSpeed)
        {
            this.playerCore = playerCore;
            this.player = player;
            this.playerController = playerController;
            this.forwardMovementSpeed = forwardMovementSpeed;
            this.sideMovementSpeed = sideMovementSpeed;
        }
        public void Enter()
        {
            //TODO - Gradually increase speed to sprint speed, then stay constant sprint speed
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
            SprintMovement();
            ApplyMovement();
        }
        public void LateTick()
        {
            playerCore.MouseMovement();
        }
        private void SprintJump()
        {
            //TODO - Code Sprint Jump Behaviour
        }
        private void ChangeStateBehaviour()
        {
            //While in SprintState
            if (playerCore.isCrouching) //Starts crouching
            {
                playerCore.ChangeState(playerCore.CrouchState); //Change to CrouchState
                playerCore.isSprinting = !playerCore.isSprinting;
            }
            else if (!playerCore.isSprinting) //Stops sprinting
            {
                playerCore.ChangeState(playerCore.WalkState); //Change to WalkState
            }
            else if (!playerCore.isWalking) //Stops walking
            {
                playerCore.ChangeState(playerCore.IdleState); //Change to IdleState
                playerCore.isSprinting = !playerCore.isSprinting; //Disable Sprint (Will be bug if not disabled, you do not need to toggle sprint to stop moving, therefore we need to disable sprint)
            }
        }
        private void SprintMovement()
        {
            float correctMovementSpeed = 0f;
            Vector2 targetPos = new Vector2(playerCore.movement.ReadValue<Vector2>().x, playerCore.movement.ReadValue<Vector2>().y);
            if (targetPos.y < 0f)
            {
                correctMovementSpeed = sideMovementSpeed;
            }
            else
            {
                correctMovementSpeed = forwardMovementSpeed;
            }
            playerCore.direction = Vector2.SmoothDamp(playerCore.direction, targetPos, ref playerCore.directionVelocity, playerCore.movementSmoothTime);
            playerCore.velocity = (player.transform.forward * playerCore.direction.y * correctMovementSpeed) + (player.transform.right * playerCore.direction.x * sideMovementSpeed) + (Vector3.up * playerCore.velocityY);
        }
        private void ApplyMovement()
        {
            playerController.Move(playerCore.velocity * Time.deltaTime);
        }
    }
}
