using Ink.Runtime;
using NobunAtelier;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

/// <summary>
/// EXTERNAL Audio_Delay(duration)
/// EXTERNAL Audio_SoundEffectPlay(soundName)
/// EXTERNAL Audio_MusicPlay(musicName)
/// EXTERNAL Audio_MusicStop(musicName)
/// </summary>

namespace NobunAtelier.Story
{

    [DisplayName("Story Module: Audio")]
    public class AudioStoryModule : StoryManagerModuleWithDelay
    {
        public override bool ShouldUpdate => false;

        [Header("Audio")]
        [SerializeField]
        private StoryAudioCollection m_audioCollection;

        [SerializeField]
        private string m_soundEffectPlayCommandName = "Audio_SoundEffectPlay";

        [SerializeField]
        private string m_musicPlayCommandName = "Audio_MusicPlay";

        [SerializeField]
        private string m_musicStopCommandName = "Audio_MusicStop";

        public override void InitModule(StoryManager moduleOwner)
        {
            base.InitModule(moduleOwner);
        }

        public override void StoryStart(Ink.Runtime.Story story, string knot)
        {
            base.StoryStart(story, knot);
            BindCommand(story, m_soundEffectPlayCommandName, (string name) => { PlaySound(name); });
            BindCommand(story, m_musicPlayCommandName, (string name) => { PlayMusic(name); });
            BindCommand(story, m_musicStopCommandName, (string name) => { StopMusic(name); });
        }

        public override void StoryUpdate(string text, IReadOnlyList<string> tags)
        { }

        public override void StoryEnd(Ink.Runtime.Story story)
        {
            base.StoryEnd(story);

            UnbindCommand(story, m_soundEffectPlayCommandName);
            UnbindCommand(story, m_musicPlayCommandName);
            UnbindCommand(story, m_musicStopCommandName);
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
}