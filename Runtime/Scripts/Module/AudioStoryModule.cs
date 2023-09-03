using Ink.Runtime;
using NobunAtelier;
using System.ComponentModel;
using UnityEngine;

[DisplayName("Story Module: Audio")]
public class AudioStoryModule : StoryManagerModuleBase
{
    [Header("Audio")]
    [SerializeField]
    private StoryAudioCollection m_audioCollection;

    [SerializeField]
    private string m_playAudioFunctionName = "PlaySound";

    [SerializeField]
    private string m_playAudioWithFadeInFunctionName = "PlayMusic";

    [SerializeField]
    private string m_stopAudioWitFadeOutFunctionName = "StopMusic";

    public override void InitModule(StoryManager moduleOwner)
    {
        base.InitModule(moduleOwner);
    }

    public override void StoryStart(Story story, string knot)
    {
        if (story.TryGetExternalFunction(m_playAudioFunctionName, out var ext1))
        {
            story.BindExternalFunction(m_playAudioFunctionName, (string name) =>
            {
                if (m_logDebug)
                {
                    Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{m_playAudioFunctionName}({name})' triggered.");
                }
                PlaySound(name);
            });
        }

        if (story.TryGetExternalFunction(m_playAudioWithFadeInFunctionName, out var ext2))
        {
            story.BindExternalFunction(m_playAudioWithFadeInFunctionName, (string name) =>
            {
                if (m_logDebug)
                {
                    Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{m_playAudioWithFadeInFunctionName}({name})' triggered.");
                }
                PlayMusic(name);
            });
        }

        if (story.TryGetExternalFunction(m_stopAudioWitFadeOutFunctionName, out var ext3))
        {
            story.BindExternalFunction(m_stopAudioWitFadeOutFunctionName, (string name) =>
            {
                if (m_logDebug)
                {
                    Debug.Log($"[{this.name}]<{this.GetType().Name}>: '{m_stopAudioWitFadeOutFunctionName}({name})' triggered.");
                }
                StopMusic(name);
            });
        }
    }

    public override void StoryEnd(Story story)
    {
        if (story.TryGetExternalFunction(m_playAudioFunctionName, out var ext1))
        {
            story.UnbindExternalFunction(m_playAudioFunctionName);
        }

        if (story.TryGetExternalFunction(m_playAudioWithFadeInFunctionName, out var ext2))
        {
            story.UnbindExternalFunction(m_playAudioWithFadeInFunctionName);
        }

        if (story.TryGetExternalFunction(m_stopAudioWitFadeOutFunctionName, out var ext3))
        {
            story.UnbindExternalFunction(m_stopAudioWitFadeOutFunctionName);
        }
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