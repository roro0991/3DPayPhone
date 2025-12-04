using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

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
}

[System.Serializable]
public class SentenceWordEntry
{
    public Word Word; // semantic object
    public string Surface; // the actual form used (ie. dog vs dogs)
    public bool hasArticle;
}

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

// Define a class to hold info about each word
[System.Serializable]
public class Word
{    
    public string Text;                     // e.g., "run"    
    //public SentenceWordEntry SentenceWordEntry;
    public PartsOfSpeech PartOfSpeech;      // bitflags for multiple roles

    public List<NounForms> NounFormsList = new();
    public List<VerbForms> VerbFormsList = new();

    // Constructor
    public Word(string text, PartsOfSpeech partOfSpeech)
    {
        Text = text;
        PartOfSpeech = partOfSpeech;
    }

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

public static class SmartSentenceParser
{
    /// <summary>
    /// Represents a parsed word with guessed POS and optional verb form.
    /// </summary>
    public class ParsedWord
    {
        public string Text;
        public PartsOfSpeech POS;
        public string VerbForm; // if applicable

        public ParsedWord(string text, PartsOfSpeech pos, string verbForm = null)
        {
            Text = text;
            POS = pos;
            VerbForm = verbForm;
        }

        public override string ToString()
        {
            return VerbForm != null
                ? $"'{Text}' -> {POS} (VerbForm: {VerbForm})"
                : $"'{Text}' -> {POS}";
        }
    }

    /// <summary>
    /// Parses a sentence and guesses POS and verb forms.
    /// </summary>
    public static List<ParsedWord> ParseSentence(string sentence)
    {
        var wdb = WordDataBase.Instance;
        var results = new List<ParsedWord>();
        if (string.IsNullOrEmpty(sentence)) return results;

        char[] delimiters = { ' ', ',', '.', ';', '!', '?', ':', '-' };
        string[] words = sentence.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

        int state = 0; // 0=subject,1=verb,2=object/modifier
        PartsOfSpeech subjectPOS = PartsOfSpeech.None;

        foreach (var rawWord in words)
        {
            string key = rawWord.ToLower();
            if (!wdb.Words.TryGetValue(key, out Word word))
            {
                results.Add(new ParsedWord(rawWord, PartsOfSpeech.None));
                continue;
            }

            PartsOfSpeech assignedPOS = PartsOfSpeech.None;
            string verbForm = null;

            switch (state)
            {
                case 0: // Expecting subject
                    if (word.HasPartOfSpeech(PartsOfSpeech.Article))
                    {
                        assignedPOS = PartsOfSpeech.Article;
                    }
                    else if (word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun | PartsOfSpeech.Noun))
                    {
                        assignedPOS = word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun)
                            ? PartsOfSpeech.SubjectPronoun
                            : PartsOfSpeech.Noun;

                        // Remember subject type for verb agreement
                        subjectPOS = assignedPOS;
                        state = 1;
                    }
                    else
                    {
                        assignedPOS = word.PartOfSpeech;
                    }
                    break;

                case 1: // Expecting verb
                    if (word.HasPartOfSpeech(PartsOfSpeech.Verb))
                    {
                        assignedPOS = PartsOfSpeech.Verb;
                        verbForm = GetVerbFormForSubject(word, subjectPOS);
                        state = 2;
                    }
                    else
                    {
                        assignedPOS = word.PartOfSpeech;
                    }
                    break;

                case 2: // Expecting object/modifier
                    if (word.HasPartOfSpeech(PartsOfSpeech.Article))
                    {
                        assignedPOS = PartsOfSpeech.Article;
                    }
                    else if (word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun | PartsOfSpeech.Noun))
                    {
                        assignedPOS = word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun)
                            ? PartsOfSpeech.ObjectPronoun
                            : PartsOfSpeech.Noun;
                    }
                    else if (word.HasPartOfSpeech(PartsOfSpeech.Adjective | PartsOfSpeech.Adverb))
                    {
                        assignedPOS = word.PartOfSpeech & (PartsOfSpeech.Adjective | PartsOfSpeech.Adverb);
                    }
                    else
                    {
                        assignedPOS = word.PartOfSpeech;
                    }
                    break;
            }

            results.Add(new ParsedWord(rawWord, assignedPOS, verbForm));
        }

        return results;
    }

    /// <summary>
    /// Chooses the correct verb form based on the subject's part of speech.
    /// </summary>
    private static string GetVerbFormForSubject(Word verbWord, PartsOfSpeech subjectPOS)
    {
        if (!verbWord.HasPartOfSpeech(PartsOfSpeech.Verb)) return null;
        var forms = verbWord.VerbFormsList.Count > 0 ? verbWord.VerbFormsList[0] : null;
        if (forms == null) return null;

        // Simple rule: use pronoun to select correct verb form
        switch (subjectPOS)
        {
            case PartsOfSpeech.SubjectPronoun:
                string subjText = verbWord.Text.ToLower();
                // Check common pronouns in WordDatabase
                if (subjText == "i") return forms.FirstPersonSingular ?? forms.Base;
                if (subjText == "you") return forms.SecondPersonSingular ?? forms.Base;
                if (subjText == "he" || subjText == "she" || subjText == "it") return forms.ThirdPersonSingular ?? forms.ThirdPerson ?? forms.Base;
                if (subjText == "we") return forms.FirstPersonPlural ?? forms.Base;
                if (subjText == "they") return forms.ThirdPersonPlural ?? forms.Base;
                break;
        }

        return forms.Base; // fallback to base form
    }

    /// <summary>
    /// Prints the parsed sentence in the console.
    /// </summary>
    public static void PrintParsedSentence(string sentence)
    {
        var parsed = ParseSentence(sentence);
        foreach (var pw in parsed)
        {
            Debug.Log(pw.ToString());
        }
    }
}



