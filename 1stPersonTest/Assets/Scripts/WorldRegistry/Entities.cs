using Ink.Parsed;
using UnityEngine;
using System.Collections.Generic;

namespace Game.World
{
    public class Entity
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
