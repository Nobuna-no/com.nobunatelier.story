using System.ComponentModel;
using UnityEngine;

/// <summary>
/// EXTERNAL screen_wait(duration)
/// EXTERNAL screen_fade_enter(duration)
/// EXTERNAL screen_fade_exit(duration)
/// </summary>

namespace NobunAtelier.Story
{
    [DisplayName("Story Module: Screen Effect")]
    public class ScreenEffectStoryModule : StoryManagerModuleWithDelay
    {
        public override bool ShouldUpdate => false;

        [Header("Screen Effect")]
        [SerializeField] private string m_screenFadeEnterCommandName = "screen_fade_enter";

        [SerializeField] private string m_screenFadeExitCommandName = "screen_fade_exit";

        public override void StoryStart(Ink.Runtime.Story story, string knot)
        {
            Debug.Assert(ScreenFader.Instance, $"An {typeof(ScreenFader).Name} is required to use {typeof(ScreenEffectStoryModule).Name}.");

            base.StoryStart(story, knot);

            BindCommand(story, m_screenFadeEnterCommandName, (float duration) =>
            {
                if (duration == 0)
                {
                    ScreenFader.Fill();
                }
                else
                {
                    CommandChannelQueueDelay(duration);
                    ScreenFader.FadeIn(duration, CommandChannelsBreakDelay);
                }
            });

            BindCommand(story, m_screenFadeExitCommandName, (float duration) =>
            {
                if (duration == 0)
                {
                    ScreenFader.Clear();
                }
                else
                {
                    CommandChannelQueueDelay(duration);
                    ScreenFader.FadeOut(duration, CommandChannelsBreakDelay);
                }
            });
        }

        public override void StoryEnd(Ink.Runtime.Story story)
        {
            base.StoryEnd(story);
            UnbindCommand(story, m_screenFadeEnterCommandName);
            UnbindCommand(story, m_screenFadeExitCommandName);
        }
    }
}