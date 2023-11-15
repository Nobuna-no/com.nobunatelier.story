using System.Collections.Generic;
using UnityEngine;

namespace NobunAtelier.Story
{
    public static class StoryActorUtility
    {
        private static readonly Dictionary<string, ActorData> s_actorMap = new Dictionary<string, ActorData>();
        private static readonly Dictionary<string, StoryActorDefinition.Attribute> s_actorAttributesMap = new Dictionary<string, StoryActorDefinition.Attribute>();

#if UNITY_EDITOR

        [UnityEditor.InitializeOnEnterPlayMode]
        private static void Init()
        {
            if (s_actorMap != null)
            {
                s_actorMap.Clear();
            }
            if (s_actorAttributesMap != null)
            {
                s_actorAttributesMap.Clear();
            }
        }

#endif

        public static bool TryGetCharacter(string id, out StoryActorDefinition character)
        {
            character = null;
            if (s_actorMap.ContainsKey(id))
            {
                character = s_actorMap[id].Definition;
            }

            return character != null;
        }

        public static void RegisterCollection(StoryActorCollection collection)
        {
            foreach (var actor in collection.GetData())
            {
                string actorId = actor.InkActorId;

                if (!s_actorMap.ContainsKey(actorId))
                {
                    s_actorMap.Add(actorId,
                        new ActorData
                        {
                            Definition = actor,
                            DefaultAttributePerType = new Dictionary<string, StoryActorDefinition.Attribute>()
                        });
                }

                foreach (var attributeArray in actor.Attributes)
                {
                    string typeId = attributeArray.AttributeTypeId;

                    foreach (var attribute in attributeArray.Attributes)
                    {
                        var key = actorId + typeId + attribute.AttributeId;

                        if (!s_actorAttributesMap.ContainsKey(key))
                        {
                            s_actorAttributesMap.Add(key, attribute);
                        }
                    }

                    // Adds first occurrence of an attribute type to the default map.
                    if (attributeArray.Attributes.Length > 0 && !s_actorMap[actorId].DefaultAttributePerType.ContainsKey(typeId))
                    {
                        s_actorMap[actorId].DefaultAttributePerType.Add(typeId, attributeArray.Attributes[0]);
                    }
                }
            }
        }

        // Resource management can be greatly improve, but for now this should do it.
        // Could be improve by keeping track of all the asset reference handle with a map of asset/handle.
        public static void ReleaseCollection(StoryActorCollection characterCollection)
        {
            foreach (var actor in characterCollection.GetData())
            {
                string actorId = actor.InkActorId;
                foreach (var attributeArray in actor.Attributes)
                {
                    string typeId = attributeArray.AttributeTypeId;

                    foreach (var attribute in attributeArray.Attributes)
                    {
                        var key = actorId + typeId + attribute.AttributeId;

                        if (s_actorAttributesMap.ContainsKey(key))
                        {
                            if (s_actorAttributesMap[key].Sprite.IsValid())
                            {
                                s_actorAttributesMap[key].Sprite.ReleaseAsset();
                            }
                        }
                    }
                }
            }

            Resources.UnloadUnusedAssets();
        }

        public static void ReleaseAllCollection()
        {
            foreach (var attribute in s_actorAttributesMap.Values)
            {
                if (attribute.Sprite.IsValid())
                {
                    attribute.Sprite.ReleaseAsset();
                }
            }

            s_actorAttributesMap.Clear();
            Resources.UnloadUnusedAssets();
        }

        public static void ReleaseActorResources(StoryActorDefinition definition)
        {
            if (!s_actorMap.ContainsKey(definition.InkActorId))
            {
                return;
            }

            foreach (var actorAndAttributes in s_actorAttributesMap)
            {
                if (actorAndAttributes.Key != definition.InkActorId)
                {
                    continue;
                }

                if (actorAndAttributes.Value.Sprite.IsValid())
                {
                    actorAndAttributes.Value.Sprite.ReleaseAsset();
                }
            }
        }

        public static bool TryGetAttribute(string actorId, string attributeTypeId, string attributeId, out Sprite sprite)
        {
            sprite = null;
            string hash = actorId + attributeTypeId + attributeId;

            if (s_actorAttributesMap.TryGetValue(hash, out var attribute))
            {
                if (!attribute.Sprite.IsValid())
                {
                    var handle = attribute.Sprite.LoadAssetAsync();
                    sprite = handle.WaitForCompletion();
                }
                else
                {
                    sprite = attribute.Sprite.Asset as Sprite;
                }

                return true;
            }
            else if (s_actorMap.TryGetValue(actorId, out var actorData)
                && actorData.DefaultAttributePerType.TryGetValue(attributeTypeId, out var attributeData))
            {
                // If attributeId is not specified, trying to return the registered attribute.
                if (!attributeData.Sprite.IsValid())
                {
                    var handle = attributeData.Sprite.LoadAssetAsync();
                    sprite = handle.WaitForCompletion();
                }
                else
                {
                    sprite = attributeData.Sprite.Asset as Sprite;
                }
                return true;
            }

            Debug.LogError($"[StoryActorUtility]: Not able to find a sprite to load from the following ids: actor=\"{actorId}\", type=\"{attributeTypeId}\", attribute=\"{attributeId}\"..");

            return false;
        }

        private class ActorData
        {
            public StoryActorDefinition Definition;
            public Dictionary<string, StoryActorDefinition.Attribute> DefaultAttributePerType;
        }
    }
}