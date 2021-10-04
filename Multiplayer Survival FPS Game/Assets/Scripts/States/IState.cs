using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridJK.MultiplayerSurvival
{
    public interface IState
    {
        public void Enter();
        public void Tick();
        public void LateTick();
        public void FixedTick();
        public void Exit();
    }
}
