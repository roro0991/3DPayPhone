using Game.World;
using UnityEngine;

public class WorldRegistryBootStrapper : MonoBehaviour
{
    public static WorldRegistry World;

    public static Entity JohnEntity;
    public static Entity CarEntity;

    private void Awake()
    {
        World = new WorldRegistry();

        JohnEntity = new Person { Id = "john", Name = "John" };
        CarEntity = new ObjectEntity { Id = "car", Name = "Red Car" };        

        World.Register(JohnEntity);
        World.Register(CarEntity);
    }
}
