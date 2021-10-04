using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJK.MultiplayerSurvival.Player;

namespace HybridJK.MultiplayerSurvival.State
{
    public class CrouchState : IState
    {
        private PlayerCore playerCore;
        private Transform player;
        private CharacterController playerController;
        private float movementSpeed;

        public CrouchState(PlayerCore playerCore, Transform player, CharacterController playerController, float movementSpeed)
        {
            this.playerCore = playerCore;
            this.player = player;
            this.playerController = playerController;
            this.movementSpeed = movementSpeed;
        }
        public void Enter()
        {
            //TODO - Drop camera height, and also gradually decrease speed until crouch speed, then stay constant crouch speed
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
            UpdateVelocity();
            ApplyMovement();
        }
        public void LateTick()
        {
            playerCore.MouseMovement();
        }
        private void CrouchJump()
        {
            //TODO - Code Crouch Jump Behaviour
        }
        private void ChangeStateBehaviour()
        {
            //While in CrouchState
            if (playerCore.isSprinting) //Starts sprinting
            {
                playerCore.ChangeState(playerCore.SprintState); //Change to sprint state
                playerCore.isCrouching = !playerCore.isCrouching;
            }
            else if (!playerCore.isCrouching) //Stops crouching
            {
                if (playerCore.isWalking) //Player is walking
                {
                    playerCore.ChangeState(playerCore.WalkState); //Change to WalkState
                }
                else //Player is not walking
                {
                    playerCore.ChangeState(playerCore.IdleState); //Change to IdleState
                }
            }
        }
        private void UpdateVelocity()
        {
            Vector2 targetPos = new Vector2(playerCore.movement.ReadValue<Vector2>().x, playerCore.movement.ReadValue<Vector2>().y);
            playerCore.direction = Vector2.SmoothDamp(playerCore.direction, targetPos, ref playerCore.directionVelocity, playerCore.movementSmoothTime);
            playerCore.velocity = (player.transform.forward * playerCore.direction.y + player.transform.right * playerCore.direction.x) * movementSpeed;
        }
        private void ApplyMovement()
        {
            playerController.Move(playerCore.velocity * Time.deltaTime);
        }
    }
}
