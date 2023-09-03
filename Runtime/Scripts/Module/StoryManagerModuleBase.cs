using Ink.Runtime;
using System.Collections.Generic;
using UnityEngine;

public abstract class StoryManagerModuleBase : MonoBehaviour
{
    public StoryManager StoryManager => m_moduleOwner;
    public bool IsRunning => m_isRunning;

    [Header("Story Manager Module")]
    [SerializeField]
    protected bool m_logDebug = false;

    private StoryManager m_moduleOwner;
    private bool m_isRunning = false;

    public virtual bool CanBeExecuted()
    {
        return this.isActiveAndEnabled;
    }

    public virtual void InitModule(StoryManager moduleOwner)
    {
        m_moduleOwner = moduleOwner;
        m_isRunning = false;
    }

    public virtual void StoryStart(Story story, string knot)
    {
        m_isRunning = true;

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

        if (m_logDebug)
            Debug.Log($"[{this.name}]<{this.GetType().Name}>: StoryEnd.");
    }
}