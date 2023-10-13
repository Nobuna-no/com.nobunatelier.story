using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace NobunAtelier.Story
{
    public class StoryManagerModuleWithDelay : StoryManagerModuleBase
    {
        protected override int ChannelCount { get; } = 1;

        [Header("Delay Command")]
        [SerializeField]
        private string m_delayCommandName = "Delay";

        public override void StoryStart(Ink.Runtime.Story story, string knot)
        {
            base.StoryStart(story, knot);

            BindCommand(story, m_delayCommandName, (float duration) =>
            {
                CommandChannelDelay(duration, 0);
            });
        }

        public override void StoryEnd(Ink.Runtime.Story story)
        {
            base.StoryEnd(story);

            story.UnbindExternalFunction(m_delayCommandName);
        }
    }
}
