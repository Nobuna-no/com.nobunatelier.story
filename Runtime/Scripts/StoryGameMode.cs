using NobunAtelier;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NobunAtelier.Story
{
    public class StoryGameMode : GameModeManager
    {
        [Header("State Machine")]
        [SerializeField]
        private GameModeStateMachine m_stateMachine;

        public override void GameModeInit()
        {
            base.GameModeInit();

            m_stateMachine.StartFromScratch();
        }
    }
}