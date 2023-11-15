using System.Collections.Generic;
using UnityEngine;

namespace NobunAtelier.Story
{
    [CreateAssetMenu(menuName = "NobunAtelier/Collection/StoryAudio", fileName = "DC_StoryAudio")]
    public class StoryAudioCollection : DataCollection<StoryAudioDefinition>
    {
        public List<StoryAudioDefinition> List => m_dataDefinitions;
    }
}