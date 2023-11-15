using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

// TODO: Ideally, this class should have a StoryActorDefinition parent that abstracts
// - InkID: identifier use to bridge Ink actor id to the codebase.
// - DisplayName: aka SpeakerName use in dialogue.
// - StoryActor: Prefab root of the UGUI object to instantiate.
namespace NobunAtelier.Story
{
    public class StoryActorDefinition : DataDefinition
    {
        public string InkActorId => m_actorId;
        public string DisplayName => m_displayName;
        public IReadOnlyList<AttributeArray> Attributes => m_attributes;

        [Tooltip("ActorId is use in Ink script to select an actor using the 'actor(actorId)' command.")]
        [SerializeField] private string m_actorId;

        [Tooltip("Name displayed in the game UI.")]
        [SerializeField] private string m_displayName;

        [InfoBox("Setting attributes in Ink script:\n" +
            "\t~ actor(\"<actor-id>\")\n" +
            "\t~ actor_attributes(\"<attribute-type-id>:<attribute-id>\")")]
        [SerializeField] private AttributeArray[] m_attributes;

        [Serializable]
        public class Attribute
        {
            public string AttributeId => m_attributeId;
            public AssetReferenceSprite Sprite => m_sprite;

            [SerializeField] private string m_attributeId;

            [SerializeField] private AssetReferenceSprite m_sprite;
        }

        [Serializable]
        public class AttributeArray
        {
            public string AttributeTypeId => m_attributeTypeId;
            public Attribute[] Attributes => m_attributes;

            [Tooltip("When using 'actor_attributes(attribute)', AttributeTypeId is used as first argument." +
                "i.e. 'actor_attributes(\"<AttributeTypeId>:<AttributeId>\")'.")]
            [SerializeField] private string m_attributeTypeId;

            [SerializeField] private Attribute[] m_attributes;
        }
    }
}