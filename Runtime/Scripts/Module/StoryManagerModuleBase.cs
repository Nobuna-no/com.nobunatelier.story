using Ink.Runtime;
using System.Collections.Generic;
using UnityEngine;

public abstract class StoryManagerModuleBase : MonoBehaviour
{
    public StoryManager StoryManager => m_moduleOwner;
    public bool IsRunning => m_isRunning;
    public virtual bool ShouldUpdate => true;

    [Header("Story Manager Module")]
    [SerializeField]
    private string m_delayCommandName = "Delay";

    [SerializeField]
    protected bool m_logDebug = false;

    private StoryManager m_moduleOwner;
    private bool m_isRunning = false;

    private Queue<System.Action> m_commandBuffer = new Queue<System.Action>();
    private bool m_isPaused = false;
    private float m_pauseRemainingDuration = 0;

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

    public virtual void StoryStart(Story story, string knot)
    {
        m_isRunning = true;
        this.enabled = true;

        BindCommand(story, m_delayCommandName, (float duration) =>
        {
            DelayCommand(duration);
        });

        if (m_logDebug)
            Debug.Log($"[{this.name}]<{this.GetType().Name}>: StoryStart({knot}).");
    }

    public virtual void StoryUpdate(string text, IReadOnlyList<string> tags)
    {
        if (m_logDebug)
            Debug.Log($"[{this.name}]<{this.GetType().Name}>: StoryUpdate.");
    }

    public virtual void StoryEnd(Story story)
    {
        m_isRunning = false;

        m_commandBuffer.Clear();
        story.UnbindExternalFunction(m_delayCommandName);

        if (m_logDebug)
            Debug.Log($"[{this.name}]<{this.GetType().Name}>: StoryEnd.");

        this.enabled = false;
    }

    public void AddDelayCommand(float duration)
    {
        m_commandBuffer.Enqueue(() => { DelayCommand(duration); });
    }

    protected virtual void DelayCommand(float duration)
    {
        m_pauseRemainingDuration += duration;
        m_isPaused = true;
    }

    public virtual void BreakDelay()
    {
        m_pauseRemainingDuration = 0;
        m_isPaused = false;
    }

    protected void BindCommand(Story story, string commandName, System.Action callback)
    {
        story.BindExternalFunction(commandName, () =>
        {
            if (m_logDebug)
            {
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}()' triggered.");
            }

            m_commandBuffer.Enqueue(() => { callback?.Invoke(); });
        });
    }

    protected void UnbindCommand(Story story, string commandName)
    {
        story.UnbindExternalFunction(commandName);
    }

    protected void BindCommand<A>(Story story, string commandName, System.Action<A> callback)
    {
        story.BindExternalFunction(commandName, (A arg1) =>
        {
            if (m_logDebug)
            {
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}({arg1})' triggered.");
            }

            m_commandBuffer.Enqueue(() => { callback?.Invoke(arg1); });
        });
    }

    protected void BindCommand<A, B>(Story story, string commandName, System.Action<A, B> callback)
    {
        story.BindExternalFunction(commandName, (A arg1, B arg2) =>
        {
            if (m_logDebug)
            {
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}({arg1}, {arg2})' triggered.");
            }

            m_commandBuffer.Enqueue(() => { callback?.Invoke(arg1, arg2); });
        });
    }

    protected void BindCommand<A,B,C>(Story story, string commandName, System.Action<A, B, C> callback)
    {
        story.BindExternalFunction(commandName, (A arg1, B arg2, C arg3) =>
        {
            if (m_logDebug)
            {
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{commandName}({arg1}, {arg2}, {arg3})' triggered.");
            }

            m_commandBuffer.Enqueue(() => { callback?.Invoke(arg1, arg2, arg3); });
        });
    }

    private void FixedUpdate()
    {
        if (!m_isPaused && m_commandBuffer.Count == 0)
        {
            return;
        }

        if (m_isPaused)
        {
            m_pauseRemainingDuration -= Time.fixedDeltaTime;
            if (m_pauseRemainingDuration <= 0)
            {
                m_pauseRemainingDuration = 0;
                m_isPaused = false;
            }
            else
            {
                return;
            }
        }

        if (m_commandBuffer.Count > 0)
        {
            for (int i = 0; i < m_commandBuffer.Count; i++)
            {
                m_commandBuffer.Dequeue()?.Invoke();
                // if a Delay is call, we stop the dequeue here.
                if (m_isPaused)
                {
                    return;
                }
            }
        }
    }
}