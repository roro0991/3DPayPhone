using System.Collections.Generic;
using UnityEngine;
using Dialogue.Core;
using Game.World;
using NUnit.Framework.Constraints;

public abstract class Contact : MonoBehaviour
{
    [SerializeField] private AddressBook addressBook;  // Must be assigned in Inspector
    [SerializeField] private PhoneNumberManager phoneNumberManager;

    public Dictionary<ResponseKey, string> ResponsesByIntent = new Dictionary<ResponseKey, string>(); 

    public List<SentenceWordEntry> SentenceWords = new List<SentenceWordEntry>();
    public string ContactNumber;
    public string ContactName;
    private bool nameKnown;
    private bool numberKnown;    
    public string OpeningLine = string.Empty;
    public string PlayerInput = string.Empty;
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
        ContactName = name;
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
                nameKnown ? ContactName : "",
                numberKnown ? ContactNumber : ""
            );

            //Debug.Log($"Notifying AddressBook with: {data.Name}, {data.PhoneNumber}");
            addressBook.UpdateContact(data);
        }
    }

    public abstract void SpeakFirstLine();
    
    public abstract string GenerateResponse(InterpretedQuery interpretedQuery);

    public string HandleInterrogativeQuery(InterpretedQuery interpretedQuery)
    {
        InterrogativeType interrogative = interpretedQuery.Interrogative; 
        Entity subject = interpretedQuery.Subject;
        SentenceWordEntry verb = interpretedQuery.Verb;

        Debug.Log("Interrogative: " + interrogative);
        Debug.Log("Subject ID: " + subject.Id);
        Debug.Log("Subject type: " + subject.GetType());
        Debug.Log("Is Person: " + (subject is Person));
        Debug.Log("Verb Surface: [" + verb.Surface + "]");
        Debug.Log("Verb equals live: " + (verb.Surface == "live"));

        switch (interrogative)
        {
            case InterrogativeType.What:
                if (subject is Person &&
                subject.Id == "you" &&
                verb.Surface is "do")
                {
                    ContactResponse = $"I am a {((Person)WorldRegistryBootStrapper.World.Get(ContactName)).Occupation}";
                }
                else
                {
                    ContactResponse = "I don't have a job";
                }
                break;
            case InterrogativeType.Where:
                if (subject is Person &&
                    subject.Id == "you" &&
                    verb.Surface is "live")
                {
                    ContactResponse = $"I live in {((Person)WorldRegistryBootStrapper.World.Get(ContactName)).CityOfResidence}";
                }
                else
                {
                    ContactResponse = "I don't have a home";
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





