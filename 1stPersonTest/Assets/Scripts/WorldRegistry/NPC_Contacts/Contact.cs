using System.Collections.Generic;
using UnityEngine;
using Dialogue.Core;
using Game.World;
using NUnit.Framework.Constraints;

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
                    Entity character = WorldRegistryBootStrapper.World.Get(ContactID);
                    if (character is PersonEntity person)
                    {

                        Entity characterJob = WorldRegistryBootStrapper.World.Get(person.Job.Id);

                        if (characterJob is JobEntity job)
                        {
                            ContactResponse = $"I am {job.JobTitle}.";
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
                    string subjectId = ContactID;
                    Entity character = WorldRegistryBootStrapper.World.Get(ContactID);
                    if (character is PersonEntity person)
                    {
                        Entity characterOccupation = WorldRegistryBootStrapper.World.Get(person.Job.Id);
                        
                        if (characterOccupation is JobEntity job)
                        {
                            Entity characterCompany = WorldRegistryBootStrapper.World.Get(job.Company.Id);

                            if (characterCompany is CompanyEntity company)
                            {
                                ContactResponse = $"I work at {company.CompanyName}.";
                            }
                        }

                    }
                }
                else if (subject is PersonEntity && verb.Surface is "work")
                {
                    string subjectId = ContactID;
                    Entity character = WorldRegistryBootStrapper.World.Get(subject.Id);
                    if (character is PersonEntity person)
                    {
                        Entity characterOccupation = WorldRegistryBootStrapper.World.Get(person.Job.Id);

                        if (characterOccupation is JobEntity job)
                        {
                            Entity characterCompany = WorldRegistryBootStrapper.World.Get(job.Company.Id);

                            if (characterCompany is CompanyEntity company)
                            {
                                ContactResponse = $"{person.Name} works at {company.CompanyName}.";
                            }
                        }

                    }
                }
                else
                {
                    ContactResponse = "I don't have a job";
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





