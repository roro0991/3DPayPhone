using System.Collections.Generic;
using UnityEngine;

public class PhoneNumberManager : MonoBehaviour
{
    [SerializeField] private CallManager callManager;

    // Store phone numbers keyed by contact reference
    private readonly Dictionary<Contact, string> contactNumbers = new Dictionary<Contact, string>();

    [SerializeField] private int numberLength = 7; // Default length for phone numbers
    public int NumberLength => numberLength;

    /// <summary>
    /// Get the number for this contact, or generate one if it doesn't exist yet.
    /// Ensures uniqueness.
    /// </summary>
    public string GetOrGenerateNumber(Contact contact)
    {
        if (!contactNumbers.TryGetValue(contact, out string number))
        {
            number = GenerateUniqueRandomNumber();
            contactNumbers[contact] = number;
        }
        return number;
    }

    /// <summary>
    /// Assigns a number to a contact if they don't already have one, ensuring uniqueness.
    /// </summary>
    public string AssignNumber(Contact contact)
    {
        if (!contactNumbers.ContainsKey(contact))
        {
            string uniqueNumber = GenerateUniqueRandomNumber();
            contactNumbers[contact] = uniqueNumber;
        }
        return contactNumbers[contact];
    }

    /// <summary>
    /// Get the contact for a given number, or null if none match.
    /// </summary>
    public Contact GetContactByNumber(string number)
    {
        foreach (var kvp in contactNumbers)
        {
            if (kvp.Value == number)
                return kvp.Key;
        }
        return null;
    }

    /// <summary>
    /// Generates a unique random N-digit number that isn't already assigned.
    /// </summary>
    private string GenerateUniqueRandomNumber()
    {
        string newNumber;
        int min = (int)Mathf.Pow(10, numberLength - 1);
        int max = (int)Mathf.Pow(10, numberLength) - 1;

        do
        {
            int num = Random.Range(min, max + 1);
            newNumber = num.ToString();
        } while (contactNumbers.ContainsValue(newNumber)); // Ensure uniqueness

        return newNumber;
    }
}





