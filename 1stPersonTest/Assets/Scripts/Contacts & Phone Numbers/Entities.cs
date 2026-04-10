using UnityEngine;

namespace Game.World
{
    public class Entity
    {
        public string Id;
        public string Name;
    }

    public class Person : Entity
    {
        public string Occupation;
        public string CityOfResidence;
        public int Age;
        public Entity Vehicle;
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
