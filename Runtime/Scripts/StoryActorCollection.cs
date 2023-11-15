using System.Collections.Generic;
using UnityEngine;

namespace NobunAtelier.Story
{
    [CreateAssetMenu(menuName = "NobunAtelier/Story/Actor", fileName = "[StoryActor]")]
    public class StoryActorCollection : DataCollection<StoryActorDefinition>
    {
        public List<StoryActorDefinition> List => m_dataDefinitions;
    }
}