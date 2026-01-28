using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using Dialogue.Core;

[System.Flags]
public enum PartsOfSpeech
{
    None = 0,
    ProperNoun = 1 << 0,
    Noun = 1 << 1,
    SubjectPronoun = 1 << 2,
    ObjectPronoun = 1 << 3,
    PossessivePronoun = 1 << 4,
    Verb = 1 << 5,
    Adjective = 1 << 6,
    Adverb = 1 << 7,
    Preposition = 1 << 8,
    Interjection = 1 << 9,
    Conjunction = 1 << 10,
    Punctuation = 1 << 11,
    Article = 1 << 12,
    Interrogative = 1 << 13,
    Character = 1 << 14,

}


// Define a class to hold info about each word
[System.Serializable]
public class Word
{    
    public string Text; // e.g., "run"    
    public PartsOfSpeech PartOfSpeech; // bitflags for multiple roles
    public WordID WordID; // optional, default to WordID.None
    public Intent Intent; // optional, default to Intent.None

    public List<NounForms> NounFormsList = new();
    public List<VerbForms> VerbFormsList = new();

    // -------------------- Constructors --------------------
    public Word(string text, 
        PartsOfSpeech partOfSpeech, 
        Intent intent = Intent.None, 
        WordID wordID = WordID.None)
    {
        Text = text;
        PartOfSpeech = partOfSpeech;        
        Intent = intent;
        WordID = wordID;
    }

    // ------------------- Nested Types -------------------

    [System.Serializable]
    public class NounForms
    {
        //Nouns
        public string Singular;
        public string Plural;
    }

    [System.Serializable]
    public class VerbForms
    {
        // Verbs (basic set)
        public string Base; //e.g. "run"
        public string Past; //e.g. "ran"
        public string PastParticiple; //e.g. "run"
        public string PresentParticiple; //e.g. "running"
        public string ThirdPerson; //e.g. "runs"

        // Special cases (like "to be")
        public string FirstPersonSingular;  // I am
        public string SecondPersonSingular; // you are
        public string ThirdPersonSingular;  // he/she/it is
        public string PluralPast; // were
        public string FirstPersonPlural;    // we are
        public string SecondPersonPlural;   // you are
        public string ThirdPersonPlural;    // they are
    }

    // ------------------ Utility Methods -------------------

    public bool HasPartOfSpeech(PartsOfSpeech pos)
    {
        return (PartOfSpeech & pos) != 0;
    }

    public void AddNounForm(NounForms form)
    {
        if (form != null)
        {
            NounFormsList.Add(form);
            PartOfSpeech |= PartsOfSpeech.Noun;
        }
    }

    public void AddVerbForm(VerbForms form)
    {
        if (form != null)
        {
            VerbFormsList.Add(form);
            PartOfSpeech |= PartsOfSpeech.Verb;
        }
    }

    public NounForms GetNounForm(int index = 0) =>
        (NounFormsList.Count > index) ? NounFormsList[index] : null;

    public VerbForms GetVerbForm(int index = 0) =>
        (VerbFormsList.Count > index) ? VerbFormsList[index] : null;

    public bool IsSingular(string surfaceWord)
    {
        foreach (var nf in NounFormsList)
        {
            if (surfaceWord == nf.Singular)
                return true;
        }
        return false;
    }

    public bool IsPlural(string surfaceWord)
    {
        foreach (var nf in NounFormsList)
        {
            if (surfaceWord == nf.Plural)
                return true;
        }
        return false;
    }
}



