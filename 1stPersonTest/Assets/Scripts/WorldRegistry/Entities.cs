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
        public Entity Job;
    }

    public class JobEntity : Entity
    {
        public string JobTitle;
        public Entity Company;
    }

    public class CompanyEntity : Entity
    {
        public string CompanyName;
        public List<Entity> Employees;
    }

    public class LocationEntity : Entity
    {
        public string Address;
        public PersonEntity personPresent;
    }
}
