using UnityEngine;

namespace Game.World
{
    public class Entity
    {
        public string Id;
    }

    public class Person : Entity
    {
        public string Name;
    }

    public class LocationEntity : Entity
    {
        public Person personPresent;
    }

    public class ObjectEntity : Entity
    {
        public Person Owner;
    }
}
