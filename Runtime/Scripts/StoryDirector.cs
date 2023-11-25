 using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace NobunAtelier.Story
{
    public class StoryDirector : Singleton<StoryDirector>
    {
        private const string storyStateName = "StoryState";

        public delegate void OnStoryEndStateDelegate();

        public event OnStoryEndStateDelegate OnStoryEnded;

        public float DefaultStoryStartFadeOutDuration => m_storyFadeDuration;
        public bool AutoStoryFade => m_autoStoryFade;

        [Header("Story Director")]
        [SerializeField] private TextAsset inkAsset;
        [SerializeField] private StoryManagerModuleBase[] m_modules;

        // This settings can probably be move to a data asset.
        [Header("Story Director - Settings")]
        [SerializeField] private string m_globalWaitCommandName = "wait";
        [Tooltip("Should each story be started with a fade out and end with a fade in? Can be override on StoryGameModeState.")]
        [SerializeField] private bool m_autoStoryFade = true;
        [SerializeField] private float m_storyFadeDuration = 1f;
        [SerializeField] private float m_skippingDelayBetweenLineInSeconds = 0.2f;
        [Tooltip("Set to false in case you want to manually handle the display of the first line of a node.")]
        [SerializeField] private bool m_displayFirstLineOnStoryStart = true;

        private Ink.Runtime.Story m_inkStory;
        private string m_knotInProgress;
        private bool m_isStoryTelling = false;
        private bool m_skipEnabled = false;

#if UNITY_EDITOR
        [Header("Story Director - Debug")]
        [SerializeField] private string m_debugStory = "1a";
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        public void Debug_StartStory()
        {
            StartStory(m_debugStory);
        }
#endif

        public void DisplayNextLine()
        {
            string text;
            if (!GoToNextLine(out text))
            {
                Debug.Log($"End of the '{inkAsset.name}' story...");

                for (int i = m_modules.Length - 1; i >= 0; --i)
                {
                    if (!m_modules[i].IsRunning)
                    {
                        continue;
                    }

                    m_modules[i].StoryEnd(m_inkStory);
                }

                UnbindCommand(m_globalWaitCommandName);

                m_skipEnabled = false;
                m_isStoryTelling = false;
                OnStoryEnded?.Invoke();
                return;
            }

            for (int i = m_modules.Length - 1; i >= 0; --i)
            {
                if (!m_modules[i].IsRunning || !m_modules[i].ShouldUpdate)
                {
                    continue;
                }

                m_modules[i].StoryUpdate(text, m_inkStory.currentTags);
            }
        }

        public void Load()
        {
            string stateJson = PlayerPrefs.GetString(storyStateName, null);
            if (stateJson != null && stateJson != "")
                m_inkStory.state.LoadJson(stateJson);
        }

        [Button(enabledMode: EButtonEnableMode.Editor)]
        public void RefreshModules()
        {
            m_modules = GetComponentsInChildren<StoryManagerModuleBase>();
        }

        public void Save()
        {
            string stateJson = m_inkStory.state.ToJson();
            PlayerPrefs.SetString(storyStateName, stateJson);
        }

        public void Skip()
        {
            m_skipEnabled = true;
            StartCoroutine(SkipperTheDolphin_Coroutine());
        }

        public bool StartStory(string knot)
        {
            if (m_isStoryTelling)
            {
                Debug.LogWarning($"Trying to start knot {knot} but a story ({m_knotInProgress}) is already in progress.");
                return false;
            }

            JumpToKnot(knot);

            for (int i = 0; i < m_modules.Length; i++)
            {
                m_modules[i].StoryStart(m_inkStory, knot);
                m_isStoryTelling |= m_modules[i].IsRunning;
            }

            if (!m_isStoryTelling)
            {
                Debug.LogError($"[{this.name}]<{this.GetType().Name}>: Story started but no module running!");
            }

            BindCommand(m_globalWaitCommandName, (float duration) =>
            {
                foreach (var module in m_modules)
                {
                    if (module.IsRunning)
                    {
                        module.CommandChannelsQueueDelay(duration);
                    }
                }
            });

            if (m_displayFirstLineOnStoryStart)
            {
                DisplayNextLine();
            }

            return true;
        }

        protected override void OnSingletonAwake()
        {
            m_inkStory = new Ink.Runtime.Story(inkAsset.text);
            m_inkStory.allowExternalFunctionFallbacks = true;
            m_inkStory.onError += (msg, type) =>
            {
                if (type == Ink.ErrorType.Warning)
                    Debug.LogWarning(msg);
                else
                    Debug.LogError(msg);
            };

            Debug.Assert(m_modules != null && m_modules.Length > 0, $"[{this.name}]<{this.GetType().Name}>: no story module assigned.");

            for (int i = 0; i < m_modules.Length; i++)
            {
                Debug.Assert(m_modules[i]);
                m_modules[i].InitModule(this);
            }
        }

        private void BindCommand<A>(string commandName, System.Action<A> callback)
        {
            m_inkStory.BindExternalFunction(commandName, (A arg1) =>
            {
                callback?.Invoke(arg1);
            });
        }

        private void FixedUpdate()
        {
            if (!m_skipEnabled)
            {
                this.enabled = false;
                return;
            }

            DisplayNextLine();
        }

        private bool GoToNextLine(out string line)
        {
            line = string.Empty;

            if (m_inkStory.canContinue)
            {
                line = m_inkStory.Continue();
                line?.Trim();
                return !string.IsNullOrEmpty(line);
            }

            // We've reached the end or a choice
            return false;
        }

        private void JumpToKnot(string id)
        {
            m_inkStory.ChoosePathString(id + ".begin");
        }

        private IEnumerator SkipperTheDolphin_Coroutine()
        {
            while (m_skipEnabled)
            {
                yield return new WaitForSeconds(m_skippingDelayBetweenLineInSeconds);
                DisplayNextLine();
            }
        }

        private void UnbindCommand(string commandName)
        {
            m_inkStory.UnbindExternalFunction(commandName);
        }

#if UNITY_EDITOR
#endif
    }
}