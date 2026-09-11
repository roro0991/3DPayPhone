using UnityEngine;
using Dialogue.Core;
using Game.World;
using Game.Facts;

public abstract class Contact : MonoBehaviour
{
    [SerializeField] private AddressBook addressBook;  // Must be assigned in Inspector
    [SerializeField] private PhoneNumberManager phoneNumberManager;

    public string ContactNumber;
    public string ContactID;
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
                    string subjectID = ContactID;

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
                            ContactResponse = "I don't know.";
                        }
                    }                                                            
                }
                else
                {
                    ContactResponse = "I don't have a job";
                }
                break;
            case QueryMode.Int_Where:
                if (subject is PersonEntity &&
                    subject.Id == "you" &&
                    verb.Surface is "work")
                {
                    string subjectID = ContactID;

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





