using Ink.Runtime;
using NobunAtelier;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[DisplayName("Story Module: Audio")]
public class AudioStoryModule : StoryManagerModuleBase
{
    [Header("Audio")]
    [SerializeField]
    private StoryAudioCollection m_audioCollection;

    [SerializeField]
    private string m_delayAudioFunctionName = "DelayAudio";

    [SerializeField]
    private string m_playAudioFunctionName = "PlaySound";

    [SerializeField]
    private string m_playAudioWithFadeInFunctionName = "PlayMusic";

    [SerializeField]
    private string m_stopAudioWitFadeOutFunctionName = "StopMusic";

    private Queue<System.Action> m_CommandBuffer = new Queue<System.Action>();

    public override void InitModule(StoryManager moduleOwner)
    {
        base.InitModule(moduleOwner);
    }

    public override void StoryStart(Story story, string knot)
    {
        story.BindExternalFunction(m_playAudioFunctionName, (string name) =>
        {
            if (m_logDebug)
            {
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{m_playAudioFunctionName}({name})' triggered.");
            }
            m_CommandBuffer.Enqueue(() => { PlaySound(name); });
        });

        story.BindExternalFunction(m_playAudioWithFadeInFunctionName, (string name) =>
        {
            if (m_logDebug)
            {
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{m_playAudioWithFadeInFunctionName}({name})' triggered.");
            }
            m_CommandBuffer.Enqueue(() => { PlayMusic(name); });
        });

        story.BindExternalFunction(m_stopAudioWitFadeOutFunctionName, (string name) =>
        {
            if (m_logDebug)
            {
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{m_stopAudioWitFadeOutFunctionName}({name})' triggered.");
            }
            m_CommandBuffer.Enqueue(() => { StopMusic(name); });
        });

        story.BindExternalFunction(m_delayAudioFunctionName, (string duration) =>
        {
            if (m_logDebug)
            {
                Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{m_delayAudioFunctionName}({duration})' triggered.");
            }

            float durationInSecond = 0;
            try
            {
                durationInSecond = float.Parse(duration);
            }
            catch { }

            m_CommandBuffer.Enqueue(() => { DelayAudio(durationInSecond); });
        });

        this.enabled = true;
    }

    public override void StoryEnd(Story story)
    {
        story.UnbindExternalFunction(m_playAudioFunctionName);
        story.UnbindExternalFunction(m_playAudioWithFadeInFunctionName);
        story.UnbindExternalFunction(m_stopAudioWitFadeOutFunctionName);
        story.UnbindExternalFunction(m_delayAudioFunctionName);

        m_CommandBuffer.Clear();
        this.enabled = false;
    }

    private bool m_isPaused = false;
    private float m_pauseRemainingDuration = 0;

    private void FixedUpdate()
    {
        if (!m_isPaused && m_CommandBuffer.Count == 0)
        {
            return;
        }

        if (m_isPaused)
        {
            m_pauseRemainingDuration -= Time.fixedDeltaTime;
            if (m_pauseRemainingDuration <= 0)
            {
                m_isPaused = false;
            }
            else
            {
                return;
            }
        }

        if (m_CommandBuffer.Count > 0)
        {
            for (int i = 0; i < m_CommandBuffer.Count; i++)
            {
                m_CommandBuffer.Dequeue()?.Invoke();
                // if a DelayAudio is call, we stop the dequeue here.
                if (m_isPaused)
                {
                    return;
                }
            }
        }
    }

    private void DelayAudio(float duration)
    {
        m_pauseRemainingDuration = duration;
        m_isPaused = true;
    }

    private void PlaySound(string Id)
    {
        var sound = m_audioCollection.List.Find(x => x.Id == Id);
        Debug.Assert(sound != null);

        AudioManager.Instance?.PlayAudio(sound);
    }

    private void PlayMusic(string Id)
    {
        var music = m_audioCollection.List.Find(x => x.Id == Id);
        Debug.Assert(music != null);

        AudioManager.Instance?.FadeInAndPlayAudio(music);
    }

    private void StopMusic(string Id)
    {
        var music = m_audioCollection.List.Find(x => x.Id == Id);
        Debug.Assert(music != null);

        AudioManager.Instance?.FadeOutAndStopAudio(music);
    }
}