using Game.World;
using Game.Facts;
using UnityEngine;
using System.Collections.Generic;

public class WorldRegistryBootStrapper : MonoBehaviour
{
    public static WorldRegistry World;
    public static NarrativeRegistry NarrativeRegistry;

    public static Entity YouEntity;

    public static Entity Job_Accountant;
    public static Entity Job_Manager;

    public static Entity JohnEntity;
    public static Information John_Job;
    public static Information John_Employer;

    public static Entity AnnaEntity;
    public static Information Anna_Job;
    public static Information Anna_Employer;

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
        };


        Job_Accountant = new JobEntity
        {
            Id = "johnoccupation",
            JobTitle = "an accountant",
        };

        Job_Manager = new JobEntity
        {
            Id = "annaoccupation",
            JobTitle = "a manager"
        };

        JohnEntity = new PersonEntity
        {
            Id = "john",
            Name = "John Smith"
        };

        John_Job = new Information
        {
            InfoID = "john_job",
            NPC_Knowledge = new Dictionary<string, KnowledgeState>
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Known}
            },
            Subject = JohnEntity,
            Relationship = Relationship.WorksAs,
            Object = Job_Accountant
        };

        John_Employer = new Information
        {
            InfoID = "john_employer",
            NPC_Knowledge = new Dictionary<string, KnowledgeState>
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Known}
            },
            Subject = JohnEntity,
            Relationship = Relationship.WorksAt,
            Object = ABC_Company
        };



        AnnaEntity = new PersonEntity
        {
            Id = "anna",
            Name = "Anna Jones"
        };

        Anna_Job = new Information
        {
            InfoID = "anna_job",
            NPC_Knowledge = new Dictionary<string, KnowledgeState>
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Known}
            },
            Subject = AnnaEntity,
            Relationship = Relationship.WorksAs,
            Object = Job_Manager
        };

        Anna_Employer = new Information
        {
            InfoID = "anna_employer",
            NPC_Knowledge = new Dictionary<string, KnowledgeState>
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Known}
            },
            Subject = AnnaEntity,
            Relationship = Relationship.WorksAt,
            Object = ABC_Company
        };

        NarrativeRegistry.Register(John_Job);
        NarrativeRegistry.Register(John_Employer);
        NarrativeRegistry.Register(Anna_Job);
        NarrativeRegistry.Register(Anna_Employer);

        World.Register(YouEntity);
        World.Register(ABC_Company);
        World.Register(JohnEntity);
        World.Register(Job_Accountant);
        World.Register(AnnaEntity);
        World.Register(Job_Manager);
    }
}
