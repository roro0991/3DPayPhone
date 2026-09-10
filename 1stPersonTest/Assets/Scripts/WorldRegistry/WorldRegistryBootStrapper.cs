using Game.World;
using Game.Facts;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;

public class WorldRegistryBootStrapper : MonoBehaviour
{
    public static WorldRegistry World;
    public static NarrativeRegistry NarrativeRegistry;

    public static Entity YouEntity;

    public static Entity JohnEntity;
    public static Entity JohnOccupation;

    public static Entity AnnaEntity;
    public static Entity AnnaOccupation;

    public static Entity ABC_Company;

    private void Awake()
    {
        World = new WorldRegistry();
        NarrativeRegistry = new NarrativeRegistry();

        YouEntity = new PersonEntity { Id = "you" };

        ABC_Company = new CompanyEntity
        {
            Id = "abccompany",
            CompanyName = "ABC Company",
            Employees = new List<Entity>
            {
                JohnEntity,
                AnnaEntity
            }
        };


        JohnOccupation = new JobEntity
        {
            Id = "johnoccupation",
            JobTitle = "an accountant",
            Company = ABC_Company
        };

        AnnaOccupation = new JobEntity
        {
            Id = "annaoccupation",
            JobTitle = "a manager",
            Company = ABC_Company
        };

        AnnaEntity = new PersonEntity
        {
            Id = "anna",
            Name = "Anna Jones",
            Job = AnnaOccupation
        };

        JohnEntity = new PersonEntity
        {
            Id = "john",
            Name = "John Smith",
            Job = JohnOccupation
        };



        World.Register(YouEntity);
        World.Register(ABC_Company);
        World.Register(JohnEntity);
        World.Register(JohnOccupation);
        World.Register(AnnaEntity);
        World.Register(AnnaOccupation);
    }
}
