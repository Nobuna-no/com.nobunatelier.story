using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace NobunAtelier.Story
{
    [AddComponentMenu("NobunAtelier/States/Game Mode/Game Mode State: Story")]
    public class GameModeState_Story : StateComponent<GameModeStateDefinition, GameModeStateCollection>
    {
        [Header("Story Game Mode State")]
        [SerializeField] private string m_storyKnotToPlay;

        [Tooltip("Transition will occur after fade if available, or right at the end of the story otherwise.")]
        [SerializeField] private bool m_transitionToNextStateAfterStory = false;

        [ShowIf("m_transitionToNextStateAfterStory")]
        [SerializeField] private GameModeStateDefinition m_nextState;

        [SerializeField] private bool m_overrideGlobalFadeSettings = false;

        [ShowIf("m_overrideGlobalFadeSettings")]
        [SerializeField] private bool m_fadeOnStoryStart = true;

        [ShowIf("m_overrideGlobalFadeSettings")]
        [SerializeField] private bool m_fadeOnStoryEnd = true;

        [ShowIf("m_overrideGlobalFadeSettings")]
        [SerializeField] private float m_fadeDuration = 1f;

        public UnityEvent m_OnStoryFinished;

        public override void Enter()
        {
            base.Enter();

            if (!StoryDirector.Instance)
            {
                Debug.LogWarning("Trying to play story but no StoryManager available...");
                return;
            }

            if (ScreenFader.IsSingletonValid && ScreenFader.IsFadeIn())
            {
                if (m_overrideGlobalFadeSettings)
                {
                    if (m_fadeOnStoryStart)
                    {
                        ScreenFader.FadeOut(m_fadeDuration);
                    }
                }
                else if (StoryDirector.Instance.AutoStoryFade)
                {
                    ScreenFader.FadeOut(StoryDirector.Instance.DefaultStoryStartFadeOutDuration);
                }
            }

            if (StoryDirector.Instance.StartStory(m_storyKnotToPlay))
            {
                StoryDirector.Instance.OnStoryEnded += Instance_OnStoryEnded;
            }
        }

        private void Instance_OnStoryEnded()
        {
            StoryDirector.Instance.OnStoryEnded -= Instance_OnStoryEnded;

            m_OnStoryFinished?.Invoke();

            if (ScreenFader.IsSingletonValid && ScreenFader.IsFadeIn())
            {
                if (m_overrideGlobalFadeSettings)
                {
                    if (m_fadeOnStoryEnd)
                    {
                        ScreenFader.FadeIn(m_fadeDuration, SetNextStateAfterFade);
                    }
                }
                else if (StoryDirector.Instance.AutoStoryFade)
                {
                    ScreenFader.FadeIn(StoryDirector.Instance.DefaultStoryStartFadeOutDuration, SetNextStateAfterFade);
                }
            }
            else
            {
                SetNextStateAfterFade();
            }
        }

        private void SetNextStateAfterFade()
        {
            if (m_transitionToNextStateAfterStory && m_nextState != null)
            {
                SetState(m_nextState);
            }
        }
    }
}