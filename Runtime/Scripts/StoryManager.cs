using NaughtyAttributes;
using System.Collections;
using UnityEngine;

/// <summary>
/// EXTERNAL System_StoryPlay(nodeName)
/// EXTERNAL System_DelayCommands(duration)
/// </summary>

namespace NobunAtelier.Story
{
    public class StoryManager : SingletonManager<StoryManager>
    {
        private const string storyStateName = "StoryState";

        [Header("Story Manager")]
        [SerializeField]
        private TextAsset inkAsset;

        [SerializeField, Tooltip("Set to false in case you want to manually handle the display of the first line of a node.")]
        private bool m_displayFirstLineOnStoryStart = true;

        private Ink.Runtime.Story m_inkStory;

        private string m_knotInProgress;
        private bool m_isStoryTelling = false;
        private bool m_skipEnabled = false;

        [SerializeField]
        private StoryManagerModuleBase[] m_modules;

        [SerializeField]
        private float m_skippingDelayBetweenLineInSeconds = 0.2f;

        [SerializeField]
        private string m_delayCommandsCommandName = "System_DelayCommands";

        public delegate void OnStoryEndStateDelegate();

        public event OnStoryEndStateDelegate OnStoryEnded;

#if UNITY_EDITOR

        [SerializeField]
        private string m_debugStory = "1a";

#endif

        protected override StoryManager GetInstance()
        {
            return this;
        }

        protected override void Awake()
        {
            base.Awake();

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

            BindCommand(m_delayCommandsCommandName, (float duration) =>
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

                UnbindCommand(m_delayCommandsCommandName);

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

        private IEnumerator SkipperTheDolphin_Coroutine()
        {
            while (m_skipEnabled)
            {
                yield return new WaitForSeconds(m_skippingDelayBetweenLineInSeconds);
                DisplayNextLine();
            }
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

        private void JumpToKnot(string id)
        {
            m_inkStory.ChoosePathString(id + ".begin");
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

        private void BindCommand<A>(string commandName, System.Action<A> callback)
        {
            m_inkStory.BindExternalFunction(commandName, (A arg1) =>
            {
                callback?.Invoke(arg1);
            });
        }

        private void UnbindCommand(string commandName)
        {
            m_inkStory.UnbindExternalFunction(commandName);
        }

#if UNITY_EDITOR

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        public void Debug_StartStory()
        {
            StartStory(m_debugStory);
        }

        [Button(enabledMode: EButtonEnableMode.Editor)]
        public void RefreshModules()
        {
            m_modules = GetComponentsInChildren<StoryManagerModuleBase>();
        }

#endif
    }
}