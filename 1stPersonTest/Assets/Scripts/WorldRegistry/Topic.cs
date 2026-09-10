using UnityEngine;
using Game.World;

namespace Game.Facts
{
    public class Topic
    {
        public string FactID;
        public string EntityID;
        public Information Information;
    }

    public class Information
    {
        public Entity Subject;
        public Entity Location;
        public string Time;
    }

    public enum KnowledgeState
    {
        Unknown,
        Known,
        FalseBelief
    }

    public enum Relationship
    {
        WorksAt,
        WorksWith,
        LivesAt
    }

}
