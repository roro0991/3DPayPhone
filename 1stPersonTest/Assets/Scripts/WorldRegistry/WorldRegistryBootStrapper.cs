using Game.World;
using Game.Info;
using UnityEngine;
using System.Collections.Generic;

public class WorldRegistryBootStrapper : MonoBehaviour
{
    public static WorldRegistry World;
    public static NarrativeRegistry NarrativeRegistry;

    public static Entity YouEntity;

    public static Entity Job_Accountant;
    public static Entity Job_Manager;
    public static Entity Job_Plumber;

    public static Entity JohnEntity;
    public static Information Info_John_Job;
    public static Information Info_John_Employer;

    public static Entity AnnaEntity;
    public static Information Info_Anna_Job;
    public static Information Info_Anna_Employer;
    public static Information Belief_John_Job;

    public static Information Info_Anna_Belief_John_Job;

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
            Id = "accountantoccupation",
            JobTitle = "an accountant",
        };

        Job_Manager = new JobEntity
        {
            Id = "manageroccupation",
            JobTitle = "a manager"
        };

        Job_Plumber = new JobEntity
        {
            Id = "plumberoccupation",
            JobTitle = "a plumber"
        };

        JohnEntity = new PersonEntity
        {
            Id = "john",
            Name = "John Smith"
        };

        Info_John_Job = new Information
        {
            NPC_Knowledge = new Dictionary<string, KnowledgeState>
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Unknown}
            },
            Subject = JohnEntity,
            Relationship = Relationship.WorksAs,
            Object = Job_Accountant
        };

        Info_John_Employer = new Information
        {
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

        Info_Anna_Job = new Information
        {
            NPC_Knowledge = new Dictionary<string, KnowledgeState>
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Known}
            },
            Subject = AnnaEntity,
            Relationship = Relationship.WorksAs,
            Object = Job_Manager
        };

        Info_Anna_Employer = new Information
        {
            NPC_Knowledge = new Dictionary<string, KnowledgeState>
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Known}
            },
            Subject = AnnaEntity,
            Relationship = Relationship.WorksAt,
            Object = ABC_Company
        };

        Belief_John_Job = new Information
        {
            NPC_Knowledge = new Dictionary<string, KnowledgeState>()
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Known},
            },
            Subject = JohnEntity,
            Relationship = Relationship.WorksAs,
            Object = Job_Plumber
        };

        Info_Anna_Belief_John_Job = new Information
        {
            NPC_Knowledge = new Dictionary<string, KnowledgeState>
            {
                {"john", KnowledgeState.Known},
                {"anna", KnowledgeState.Known},
            },
            Subject = AnnaEntity,
            Relationship = Relationship.Believes,
            Object = Belief_John_Job
        };



        NarrativeRegistry.Register(Info_John_Job);
        NarrativeRegistry.Register(Info_John_Employer);
        NarrativeRegistry.Register(Info_Anna_Job);
        NarrativeRegistry.Register(Info_Anna_Employer);
        NarrativeRegistry.Register(Info_Anna_Belief_John_Job);

        World.Register(YouEntity);
        World.Register(ABC_Company);
        World.Register(JohnEntity);
        World.Register(Job_Accountant);
        World.Register(AnnaEntity);
        World.Register(Job_Manager);
        World.Register(Job_Plumber);
    }
}
