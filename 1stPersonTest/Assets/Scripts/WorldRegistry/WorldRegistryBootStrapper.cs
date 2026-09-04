using Game.World;
using Game.Facts;
using UnityEngine;

public class WorldRegistryBootStrapper : MonoBehaviour
{
    public static WorldRegistry World;
    public static NarrativeRegistry NarrativeRegistry;

    public static Entity YouEntity;
    public static Entity JohnEntity;
    public static Entity SarahEntity;
    public static Fact JohnOccupation;
    public static Fact JohnResidence;
    public static Fact SarahOccupation;

    private void Awake()
    {
        World = new WorldRegistry();
        NarrativeRegistry = new NarrativeRegistry();

        YouEntity = new Person { Id = "you" };

        JohnEntity = new Person { Id = "john", Name = "John Smith"};
        JohnOccupation = new Fact
        {
            FactID = "johnoccupation",
            EntityID = "john",
            Information = "teacher"
        };
        JohnResidence = new Fact
        {
            FactID = "johnresidence",
            EntityID = "john",
            Information = "123 Fake Street"
        };

        SarahEntity = new Person { Id = "sarah", Name = "Sarah Jones" };
        SarahOccupation = new Fact
        {
            FactID = "sarahoccupation",
            EntityID = "sarah",
            Information = "lawyer"
        };


        World.Register(YouEntity);
        World.Register(JohnEntity);
        World.Register(SarahEntity);
        NarrativeRegistry.Register(JohnOccupation);
        NarrativeRegistry.Register(JohnResidence);
        NarrativeRegistry.Register(SarahOccupation);
    }
}
