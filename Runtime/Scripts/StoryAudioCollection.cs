using System.Collections.Generic;
using UnityEngine;
using NobunAtelier;

[CreateAssetMenu(menuName = "NobunAtelier/Collection/StoryAudio", fileName = "DC_StoryAudio")]
public class StoryAudioCollection : DataCollection<StoryAudioDefinition>
{
    public List<StoryAudioDefinition> List => m_dataDefinitions;
}
