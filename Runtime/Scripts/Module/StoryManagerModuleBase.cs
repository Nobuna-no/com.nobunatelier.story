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

        // private float m_pauseRemainingDuration = 0;
        // private bool m_isPaused = false;
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
            this.enabled = true;

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

        public void CommandChannelsDelay(float duration)
        {
            for (int i = 0, c = m_commandChannels.Length; i < c; ++i)
            {
                int channel = i;
                m_commandChannels[i].Commands.Enqueue(() => { CommandChannelDelay(duration, channel); });
            }
        }

        public void CommandChannelsBreakDelay()
        {
            for (int i = 0, c = m_commandChannels.Length; i < c; ++i)
            {
                CommandChannelBreakDelay(i);
            }
        }

        public void CommandChannelQueueDelay(float duration, int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            m_commandChannels[channel].Commands.Enqueue(() => { CommandChannelDelay(duration, channel); });
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
            m_commandChannels[channel].IsPaused = true;
            m_needUpdate = true;
        }

        protected void BindCommand(Ink.Runtime.Story story, string commandName, System.Action callback, int channel = 0)
        {
            Debug.Assert(channel < m_commandChannels.Length);
            story.BindExternalFunction(commandName, () =>
            {
                if (m_logDebug)
                {
                    Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}()' triggered.");
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
                        Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}({arg1})' triggered.");
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
                        Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}({arg1}, {arg2})' triggered.");
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
                        Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}({arg1}, {arg2}, {arg3})' triggered.");
                    }
                    callback?.Invoke(arg1, arg2, arg3);
                });
            });
        }

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
                        Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}({arg1}, {arg2}, {arg3}, {arg4})' triggered.");
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
                m_commandChannels[i].Commands = new Queue<Action>();
            }
        }

        private void FixedUpdate()
        {
            if (/*!m_isPaused && */!m_needUpdate) //  m_commandBuffers.Count == 0
            {
                return;
            }

            //if (m_isPaused)
            //{
            //    m_pauseRemainingDuration -= Time.fixedDeltaTime;
            //    if (m_pauseRemainingDuration <= 0)
            //    {
            //        m_pauseRemainingDuration = 0;
            //        m_isPaused = false;
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}

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

                for (int j = 0; j < m_commandChannels[i].Commands.Count; j++)
                {
                    m_commandChannels[i].Commands.Dequeue()?.Invoke();
                    // if a Delay is call, we stop the dequeue here.
                    if (m_commandChannels[i].IsPaused)
                    {
                        m_needUpdate = true;
                        break;
                    }
                }

                // if (m_commandChannels.Count > 0)
                // {
                //     for (int i = 0; i < m_commandChannels.Count; i++)
                //     {
                //         m_commandChannels.Dequeue()?.Invoke();
                //         // if a Delay is call, we stop the dequeue here.
                //         if (m_isPaused)
                //         {
                //             return;
                //         }
                //     }
                // }
            }
        }

        private struct CommandChannel
        {
            public Queue<System.Action> Commands;
            public float PauseRemainingDuration;
            public bool IsPaused;
        }
    }
}