using System.Collections.Generic;
using UnityEngine;

public class PhoneNumberManager : MonoBehaviour
{
    [SerializeField] private CallManager callManager;

    // Store phone numbers keyed by contact reference
    private readonly Dictionary<Contact, string> contactNumbers = new Dictionary<Contact, string>();

    [SerializeField] private int numberLength = 4; // last digits after prefix
    public int NumberLength => numberLength;
    public int FullNumberLength => numberPrefix.Length + numberLength;

    [SerializeField] private string numberPrefix = "555"; // prefix for all numbers

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
        // Normalize both by removing any dashes before comparison
        string normalizedInput = number.Replace("-", "");

        foreach (var kvp in contactNumbers)
        {
            string storedNormalized = kvp.Value.Replace("-", "");
            if (storedNormalized == normalizedInput)
                return kvp.Key;
        }
        return null;
    }


    /// <summary>
    /// Generates a unique random phone number in the format prefix-XXXX.
    /// </summary>
    private string GenerateUniqueRandomNumber()
    {
        string newNumber;

        do
        {
            int lastDigits = Random.Range(0, (int)Mathf.Pow(10, numberLength)); // 0 - 9999 for 4 digits
            newNumber = $"{numberPrefix}-{lastDigits:D4}"; // pads with leading zeros
        }
        while (contactNumbers.ContainsValue(newNumber)); // Ensure uniqueness

        return newNumber;
    }
}







