using UnityEngine;
using NobunAtelier;

public class StoryAudioDefinition : AudioDefinition
{
    public string Id => m_id;

    [Header("Story")]
    [SerializeField]
    private string m_id;
}
