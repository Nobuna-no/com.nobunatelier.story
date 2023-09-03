using System.Collections.Generic;
using UnityEngine;
using NobunAtelier;

[CreateAssetMenu(menuName = "HAGJ7/Story/Audio", fileName = "[Audio] ")]
public class StoryAudioCollection : DataCollection<StoryAudioDefinition>
{
    public List<StoryAudioDefinition> List => m_dataDefinitions;
}
