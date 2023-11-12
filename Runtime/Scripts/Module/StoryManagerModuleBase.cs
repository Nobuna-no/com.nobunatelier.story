using System;
using System.Collections.Generic;
using UnityEngine;

namespace NobunAtelier.Story
{
    public abstract class StoryManagerModuleBase : MonoBehaviour
    {
        public StoryManager StoryManager => m_moduleOwner;
        public bool IsRunning => m_isRunning;
        public virtual bool ShouldUpdate => true;

        protected abstract int ChannelCount { get; }

        [Header("Story Manager Module")]
        [SerializeField]
        protected bool m_logDebug = false;

        private StoryManager m_moduleOwner;
        private bool m_isRunning = false;

        private CommandChannel[] m_commandChannels;

        private bool m_needUpdate = false;

        public virtual bool CanBeExecuted()
        {
            return this.isActiveAndEnabled;
        }

        public virtual void InitModule(StoryManager moduleOwner)
        {
            m_moduleOwner = moduleOwner;
            m_isRunning = false;
            this.enabled = false;
        }

        public virtual void StoryStart(Ink.Runtime.Story story, string knot)
        {
            m_isRunning = true;
            m_needUpdate = false;
            this.enabled = true;

            foreach (var channel in m_commandChannels)
            {
                channel.Commands.Clear();
                channel.PauseRemainingDuration = 0;
                channel.IsPaused = false;
            }

            if (m_logDebug)
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: StoryStart({knot}).");
        }

        public virtual void StoryUpdate(string text, IReadOnlyList<string> tags)
        {
            if (m_logDebug)
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: StoryUpdate.");
        }

        public virtual void StoryEnd(Ink.Runtime.Story story)
        {
            m_isRunning = false;

            for (int i = 0, c = m_commandChannels.Length; i < c; i++)
            {
                m_commandChannels[i].Commands.Clear();
            }

            if (m_logDebug)
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: StoryEnd.");

            this.enabled = false;
        }

        public void CommandChannelQueueDelay(float duration, int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            m_commandChannels[channel].Commands.Enqueue(() =>
            {
                if (m_logDebug)
                {
                    Debug.Log($"[{this.name}]<{this.GetType().Name}>: CommandChannelsQueueDelay({duration}) triggered.");
                }
                CommandChannelDelay(duration, channel);
            });
            m_needUpdate = true;
        }

        public void CommandChannelsQueueDelay(float duration)
        {
            for (int i = 0, c = m_commandChannels.Length; i < c; ++i)
            {
                int channel = i;
                m_commandChannels[i].Commands.Enqueue(() =>
                {
                    if (m_logDebug)
                    {
                        Debug.Log($"[{this.name}]<{this.GetType().Name}>: CommandChannelsQueueDelay({duration}) triggered.");
                    }
                    CommandChannelDelay(duration, channel);
                });
            }
            m_needUpdate = true;
        }

        public void CommandChannelsBreakDelay()
        {
            for (int i = 0, c = m_commandChannels.Length; i < c; ++i)
            {
                CommandChannelBreakDelay(i);
            }
        }

        protected virtual void CommandChannelDelay(float duration, int channel)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            m_commandChannels[channel].PauseRemainingDuration += duration;
            m_commandChannels[channel].IsPaused = true;
            m_needUpdate = true;
        }

        public virtual void CommandChannelBreakDelay(int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            m_commandChannels[channel].PauseRemainingDuration = 0;
            m_commandChannels[channel].IsPaused = false;
            m_needUpdate = true;
        }

        protected void BindCommand(Ink.Runtime.Story story, string commandName, System.Action callback, int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            story.BindExternalFunction(commandName, () =>
            {
                if (m_logDebug)
                {
                    Debug.Log($"[{Time.frameCount}][{this.name}]<{this.GetType().Name}>: '{commandName}()' triggered.");
                }
                m_needUpdate = true;
                m_commandChannels[channel].Commands.Enqueue(() => { callback?.Invoke(); });
            });
        }

        protected void BindCommand<A>(Ink.Runtime.Story story, string commandName, System.Action<A> callback, int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            story.BindExternalFunction(commandName, (A arg1) =>
            {
                m_needUpdate = true;
                m_commandChannels[channel].Commands.Enqueue(() =>
                {
                    if (m_logDebug)
                    {
                        Debug.Log($"[{Time.frameCount}][{this.name}]<{this.GetType().Name}>: '{commandName}({arg1})' triggered.");
                    }
                    callback?.Invoke(arg1);
                });
            });
        }

        protected void BindCommand<A, B>(Ink.Runtime.Story story, string commandName, System.Action<A, B> callback, int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            story.BindExternalFunction(commandName, (A arg1, B arg2) =>
            {
                m_needUpdate = true;
                m_commandChannels[channel].Commands.Enqueue(() =>
                {
                    if (m_logDebug)
                    {
                        Debug.Log($"[{Time.frameCount}][{this.name}]<{this.GetType().Name}>: '{commandName}({arg1}, {arg2})' triggered.");
                    }
                    callback?.Invoke(arg1, arg2);
                });
            });
        }

        protected void BindCommand<A, B, C>(Ink.Runtime.Story story, string commandName, System.Action<A, B, C> callback, int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            story.BindExternalFunction(commandName, (A arg1, B arg2, C arg3) =>
            {
                m_needUpdate = true;
                m_commandChannels[channel].Commands.Enqueue(() =>
                {
                    if (m_logDebug)
                    {
                        Debug.Log($"[{Time.frameCount}][{this.name}]<{this.GetType().Name}>: '{commandName}({arg1}, {arg2}, {arg3})' triggered.");
                    }
                    callback?.Invoke(arg1, arg2, arg3);
                });
            });
        }

        // Maximum is 4 arguments
        protected void BindCommand<A, B, C, D>(Ink.Runtime.Story story, string commandName, System.Action<A, B, C, D> callback, int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            story.BindExternalFunction(commandName, (A arg1, B arg2, C arg3, D arg4) =>
            {
                m_needUpdate = true;
                m_commandChannels[channel].Commands.Enqueue(() =>
                {
                    if (m_logDebug)
                    {
                        Debug.Log($"[{Time.frameCount}][{this.name}]<{this.GetType().Name}>: '{commandName}({arg1}, {arg2}, {arg3}, {arg4})' triggered.");
                    }
                    callback?.Invoke(arg1, arg2, arg3, arg4);
                });
            });
        }

        protected void UnbindCommand(Ink.Runtime.Story story, string commandName)
        {
            story.UnbindExternalFunction(commandName);
        }

        private void Awake()
        {
            m_commandChannels = new CommandChannel[ChannelCount];

            for (int i = 0, c = m_commandChannels.Length; i < c; ++i)
            {
                m_commandChannels[i] = new CommandChannel()
                {
                    Commands = new Queue<Action>()
                };
            }
        }

        private void FixedUpdate()
        {
            if (!m_needUpdate)
            {
                return;
            }

            // Reset the dirty status and if anything prevent all the remaining command to be executed, dirty again.
            m_needUpdate = false;
            for (int i = 0, c = m_commandChannels.Length; i < c; ++i)
            {
                if (m_commandChannels[i].IsPaused)
                {
                    m_commandChannels[i].PauseRemainingDuration -= Time.fixedDeltaTime;
                    if (m_commandChannels[i].PauseRemainingDuration <= 0)
                    {
                        m_commandChannels[i].PauseRemainingDuration = 0;
                        m_commandChannels[i].IsPaused = false;
                    }
                    else
                    {
                        m_needUpdate = true;
                        continue;
                    }
                }

                if (m_commandChannels[i].Commands.Count == 0)
                {
                    continue;
                }

                for (int j = 0, d = m_commandChannels[i].Commands.Count; j < d; j++)
                {
                    m_commandChannels[i].Commands.Dequeue()?.Invoke();
                    // if a Delay is call, we stop the dequeue here.
                    if (m_commandChannels[i].IsPaused)
                    {
                        m_needUpdate = true;
                        break;
                    }
                }
            }
        }

        private class CommandChannel
        {
            public Queue<System.Action> Commands;
            public float PauseRemainingDuration;
            public bool IsPaused;
        }
    }
}