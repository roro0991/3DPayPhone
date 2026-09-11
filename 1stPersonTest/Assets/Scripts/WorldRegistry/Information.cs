using UnityEngine;
using Game.World;
using System.Collections.Generic;

namespace Game.Facts
{
    public class Information
    {
        public string InfoID;
        public Dictionary<string, KnowledgeState> NPC_Knowledge;
        public Entity Subject;
        public Relationship Relationship;
        public Entity Object;
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
        LivesAt
    }
}
