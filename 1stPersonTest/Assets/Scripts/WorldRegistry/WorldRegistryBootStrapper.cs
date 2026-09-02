using Game.World;
using UnityEngine;

public class WorldRegistryBootStrapper : MonoBehaviour
{
    public static WorldRegistry World;

    public static Entity YouEntity;
    public static Entity JohnEntity;
    public static Entity CarEntity;

    private void Awake()
    {
        World = new WorldRegistry();

        YouEntity = new Person { Id = "you" };
        JohnEntity = new Person { Id = "John", Name = "John", Occupation = "teacher" };
        CarEntity = new ObjectEntity { Id = "car", Name = "Red Car" };

        World.Register(YouEntity);
        World.Register(JohnEntity);
        World.Register(CarEntity);
    }
}
