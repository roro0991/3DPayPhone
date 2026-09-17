using UnityEngine;
using Dialogue.Core;
using Game.World;
using Game.Info;

public abstract class Contact : MonoBehaviour
{
    [SerializeField] private AddressBook addressBook;  // Must be assigned in Inspector
    [SerializeField] private PhoneNumberManager phoneNumberManager;

    public string ContactNumber;
    public string ContactID;
    public Entity ContactEntity;
    private bool nameKnown;
    private bool numberKnown;    
    public string OpeningLine = string.Empty;
    public string ContactResponse = string.Empty;

    [SerializeField] public WordBank wordBank;

    private void Awake()
    {
        if (addressBook == null)
        {
            //Debug.LogWarning($"Contact '{ContactName}' has no AddressBook assigned in Inspector.");
        }
    }

    public void DiscoverName(string name)
    {
        ContactID = name;
        nameKnown = true;

        ContactEntity = WorldRegistryBootStrapper.World.Get(ContactID);    

        NotifyAddressBook();
    }

    public void DiscoverNumber(PhoneNumberManager phoneNumberManager)
    {
        if (!numberKnown)
        {
            ContactNumber = phoneNumberManager.GetOrGenerateNumber(this);
            numberKnown = true;
            //Debug.Log($"{ContactName} number discovered: {ContactNumber}");
        }

        NotifyAddressBook();
    }

    private void NotifyAddressBook()
    {
        if (addressBook != null)
        {
            var data = new ContactData(
                nameKnown ? ContactID : "",
                numberKnown ? ContactNumber : ""
            );

            //Debug.Log($"Notifying AddressBook with: {data.Name}, {data.PhoneNumber}");
            addressBook.UpdateContact(data);
        }
    }

    public abstract void SpeakFirstLine();
    
    public abstract string GenerateResponse(InterpretedInputData interpretedQuery);

    public string HandleDeclarative(InterpretedInputData interpretedStatement)
    {
        Entity subject = interpretedStatement.Subject;
        Entity target = interpretedStatement.Object;
        SentenceWordEntry verb = interpretedStatement.Verb;

        Debug.Log("subject: " + subject.Id);
        Debug.Log("object: " + target.Id);
        Debug.Log("verb: " + verb.Word.Text);

        Relationship relationship = new();

        if (subject is PersonEntity)
        {
            switch (verb.Word.Text)
            {
                case "work":
                    relationship = Relationship.WorksAs;
                    break;
                default:
                    break;
            }
        }

        Information info = WorldRegistryBootStrapper.NarrativeRegistry.Find(subject, relationship);

        if (info != null)
        {
            if (info.Object != null &&
                info.Object == target)
            {
                if (info.NPC_Knowledge.TryGetValue(ContactID, out KnowledgeState knowledgeState))
                {
                    if (knowledgeState == KnowledgeState.Unknown)
                    {
                        info.NPC_Knowledge[ContactID] = KnowledgeState.Known;
                        ContactResponse = "I didn't know that. Thanks for telling me.";
                    }
                    else if (knowledgeState == KnowledgeState.Known)
                    {
                        ContactResponse = "I already knew that.";
                    }
                }
            }

        }

        return ContactResponse;
    }

    public string HandleInterrogativeQuery(InterpretedInputData interpretedQuery)
    {
        QueryMode interrogative = interpretedQuery.QueryMode; 
        Entity subject = interpretedQuery.Subject;
        Entity target = interpretedQuery.Object;
        SentenceWordEntry verb = interpretedQuery.Verb;

        switch (interrogative)
        {
            case QueryMode.Int_What:
                if (subject is PersonEntity &&
                subject.Id == "you" &&
                verb.Surface is "do")
                {
                    subject = WorldRegistryBootStrapper.World.Get(ContactID);
                    Relationship relationship = Relationship.WorksAs;

                    Information info = WorldRegistryBootStrapper.NarrativeRegistry.Find(subject, relationship);

                    if (info.NPC_Knowledge.TryGetValue(ContactID, out KnowledgeState knowledgeState))
                    {
                        if (knowledgeState == KnowledgeState.Known)
                        {
                            if (info.Object is JobEntity job)
                            {
                                ContactResponse = $"I work as a {job.JobTitle}.";
                            }
                        }
                        else if (knowledgeState == KnowledgeState.Unknown)
                        {
                            ContactResponse = "I don't have a job.";
                        }
                    }                                                            
                }
                else if (subject is PersonEntity && verb.Surface is "do")
                {
                    subject = WorldRegistryBootStrapper.World.Get(subject.Id);

                    Relationship relationship = Relationship.WorksAs;

                    Information info = WorldRegistryBootStrapper.NarrativeRegistry.Find(subject, relationship);

                    if (info.NPC_Knowledge.TryGetValue(ContactID, out KnowledgeState knowledgeState))
                    {
                        if (knowledgeState == KnowledgeState.Known)
                        {
                            if (subject is PersonEntity person && info.Object is JobEntity job)
                            {
                                ContactResponse = $"{person.Name} works as {job.JobTitle}.";
                            }
                        }
                        else if (knowledgeState == KnowledgeState.Unknown)
                        {
                            // Check for a false belief in absence of knowledge of fact

                            Debug.Log($"ContactID: {ContactID}");
                            Debug.Log($"ContactEntity: {ContactEntity}");
                            Debug.Log($"Info subject: {info.Subject}");
                            Debug.Log($"Info object: {info.Object}");

                            Information belief = 
                                WorldRegistryBootStrapper.NarrativeRegistry.FindBelief(ContactEntity, subject, Relationship.WorksAs);

                            if (belief != null)
                            {
                                if (subject is PersonEntity person && belief.Object is Information infoOfBelief)
                                {
                                    if (infoOfBelief.Object is JobEntity job)
                                    {
                                        ContactResponse = $"I believe {person.Name} works as {job.JobTitle}.";
                                    }
                                }
                            }
                            else if (subject is PersonEntity person)
                            {
                                ContactResponse = $"I don't know {person.Name}'s job.";
                            }
                        }
                    }
                }
                break;
            case QueryMode.Int_Where:
                if (subject is PersonEntity &&
                    subject.Id == "you" &&
                    verb.Surface is "work")
                {
                    subject = WorldRegistryBootStrapper.World.Get(ContactID);
                    Relationship relationship = Relationship.WorksAt;

                    Information info = WorldRegistryBootStrapper.NarrativeRegistry.Find(subject, relationship);

                    if (info.Object is CompanyEntity company)
                    {
                        ContactResponse = $"I work at {company.CompanyName}.";
                    }
                }
                else if (subject is PersonEntity && verb.Surface is "work")
                {
                    subject = WorldRegistryBootStrapper.World.Get(subject.Id);

                    Relationship relationship = Relationship.WorksAt;

                    Information info = WorldRegistryBootStrapper.NarrativeRegistry.Find(subject, relationship);

                    if (subject is PersonEntity person && info.Object is CompanyEntity company)
                    {
                        ContactResponse = $"{person.Name} works at {company.CompanyName}.";
                    }
                }
                else
                {
                    ContactResponse = "I don't know.";
                }
                break;
            default:
                break;
        }

        return ContactResponse;
    }

    public virtual void PopulateWordBank()
    {
        // default: do nothing
    }
}





