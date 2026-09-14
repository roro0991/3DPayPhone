using Ink.Parsed;
using UnityEngine;
using System.Collections.Generic;
using Game.Info;

namespace Game.World
{
    public class Entity : NarrativeNode
    {
        public string Id;
    }

    public class PersonEntity : Entity
    {
        public string Name;
    }

    public class JobEntity : Entity
    {
        public string JobTitle;
    }

    public class CompanyEntity : Entity
    {
        public string CompanyName;
    }

    public class LocationEntity : Entity
    {
        public string Address;
    }
}
