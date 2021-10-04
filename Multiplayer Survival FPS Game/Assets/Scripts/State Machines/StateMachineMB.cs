using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HybridJK.MultiplayerSurvival
{
    public class StateMachineMB : NetworkBehaviour
    {
        public IState CurrentState { get; private set; }
        public IState previousState;

        private bool inTransition = false;

        public void ChangeState(IState newState)
        {
            if (CurrentState == newState || inTransition) { return; }
            ValidateAndChangeState(newState);
        }
        private void RevertState()
        {
            if (previousState != null)
            {
                ChangeState(previousState);
            }
        }
        private void ValidateAndChangeState(IState newState)
        {
            inTransition = true;
            if (CurrentState != null)
            {
                CurrentState.Exit();
                previousState = CurrentState;
            }
            CurrentState = newState;
            if (CurrentState != null)
            {
                CurrentState.Enter();
            }
            inTransition = false;
        }
        public void Update()
        {
            CurrentState.Tick();
        }
        public void LateUpdate()
        {
            CurrentState.LateTick();
        }
        public void FixedUpdate()
        {
            CurrentState.FixedTick();
        }
    }
}
