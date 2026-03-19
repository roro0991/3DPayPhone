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
        public string Singular;
        public string Plural;
    }

    [System.Serializable]
    public class VerbForms
    {
        // Regular verbs
        public string Base; // run
        public string Past; // ran
        public string PastParticiple; // run
        public string PresentParticiple; // running
        public string ThirdPerson; // runs

        public enum VerbForm
        {
            Base,
            ThirdPersonSingular,
            PresentParticiple,
            Past,
            PastParticiple
        }

        // Special cases (mainly for "to be")
        public string FirstPersonSingular;  // am
        public string SecondPersonSingular; // are
        public string ThirdPersonSingular;  // is
        public string PluralPast;           // were
        public string FirstPersonPlural;    // are
        public string SecondPersonPlural;   // are
        public string ThirdPersonPlural;    // are

        private Dictionary<string, VerbForm> lookup;

        public void BuildLookup()
        {
            lookup = new Dictionary<string, VerbForm>(StringComparer.OrdinalIgnoreCase);

            void Add(string word, VerbForm form)
            {
                if (!string.IsNullOrEmpty(word))
                    lookup[word] = form;
            }

            // Regular verbs
            Add(Base, VerbForm.Base);
            Add(ThirdPerson, VerbForm.ThirdPersonSingular);
            Add(PresentParticiple, VerbForm.PresentParticiple);
            Add(Past, VerbForm.Past);
            Add(PastParticiple, VerbForm.PastParticiple);

            // "to be" support
            Add(FirstPersonSingular, VerbForm.Base);        // am
            Add(SecondPersonSingular, VerbForm.Base);       // are
            Add(ThirdPersonSingular, VerbForm.ThirdPersonSingular); // is
            Add(PluralPast, VerbForm.Past);                 // were
            Add(FirstPersonPlural, VerbForm.Base);
            Add(SecondPersonPlural, VerbForm.Base);
            Add(ThirdPersonPlural, VerbForm.Base);
        }

        public bool TryGetForm(string word, out VerbForm form)
        {
            if (lookup == null)
                BuildLookup();

            return lookup.TryGetValue(word, out form);
        }
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



