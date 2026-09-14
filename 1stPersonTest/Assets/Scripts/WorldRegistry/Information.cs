using UnityEngine;
using Game.World;
using System.Collections.Generic;

namespace Game.Info
{
    public class Information : NarrativeNode
    {
        public Dictionary<string, KnowledgeState> NPC_Knowledge;
        public Entity Subject;
        public Relationship Relationship;
        public NarrativeNode Object;
    }

    public abstract class NarrativeNode
    {
        
    }

    public enum KnowledgeState
    {
        Unknown,
        Known
    }

    public enum Relationship
    {
        WorksAs,
        WorksAt,
        WorksWith,
        LivesAt,
        Believes
    }
}
