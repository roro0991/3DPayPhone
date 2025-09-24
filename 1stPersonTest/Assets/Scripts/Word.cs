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
}

public static class WordDatabase
{
    public static Dictionary<string, Word> Words = new Dictionary<string, Word>();

    static WordDatabase()
    {
        // ----------------- Articles -----------------
        AddWord(new Word("the", PartsOfSpeech.Article));
        AddWord(new Word("a", PartsOfSpeech.Article));
        AddWord(new Word("an", PartsOfSpeech.Article));

        // ----------------- Nouns -----------------
        var dogForms = new NounForms { Singular = "dog", Plural = "dogs" };
        var dogWord = new Word("dog", PartsOfSpeech.Noun);
        dogWord.AddNounForm(dogForms);
        AddWord(dogWord);

        // ----------------- Pronouns -----------------
        AddWord(new Word("i", PartsOfSpeech.SubjectPronoun));
        AddWord(new Word("you", PartsOfSpeech.SubjectPronoun | PartsOfSpeech.ObjectPronoun));
        AddWord(new Word("he", PartsOfSpeech.SubjectPronoun));
        AddWord(new Word("she", PartsOfSpeech.SubjectPronoun));
        AddWord(new Word("it", PartsOfSpeech.SubjectPronoun | PartsOfSpeech.ObjectPronoun));
        AddWord(new Word("we", PartsOfSpeech.SubjectPronoun));
        AddWord(new Word("they", PartsOfSpeech.SubjectPronoun));

        AddWord(new Word("me", PartsOfSpeech.ObjectPronoun));
        AddWord(new Word("him", PartsOfSpeech.ObjectPronoun));
        AddWord(new Word("her", PartsOfSpeech.ObjectPronoun));
        AddWord(new Word("us", PartsOfSpeech.ObjectPronoun));
        AddWord(new Word("them", PartsOfSpeech.ObjectPronoun));

        AddWord(new Word("mine", PartsOfSpeech.PossessivePronoun));
        AddWord(new Word("his", PartsOfSpeech.PossessivePronoun));
        AddWord(new Word("hers", PartsOfSpeech.PossessivePronoun));
        AddWord(new Word("ours", PartsOfSpeech.PossessivePronoun));
        AddWord(new Word("theirs", PartsOfSpeech.PossessivePronoun));

        // ----------------- Verbs -----------------
        var meetForms = new VerbForms
        {
            Base = "meet",
            Past = "met",
            PastParticiple = "met",
            PresentParticiple = "meeting",
            ThirdPerson = "meets"
        };
        var meetWord = new Word("meet", PartsOfSpeech.Verb);
        meetWord.AddVerbForm(meetForms);
        AddWord(meetWord);

        var beForms = new VerbForms
        {
            Base = "be",
            FirstPersonSingular = "am",
            SecondPersonSingular = "are",
            ThirdPersonSingular = "is",
            FirstPersonPlural = "are",
            SecondPersonPlural = "are",
            ThirdPersonPlural = "are",
            Past = "was",
            PluralPast = "were",
            PastParticiple = "been",
            PresentParticiple = "being"
        };
        var beWord = new Word("be", PartsOfSpeech.Verb);
        beWord.AddVerbForm(beForms);
        AddWord(beWord);

        // ----------------- Adjectives -----------------
        AddWord(new Word("nice", PartsOfSpeech.Adjective));

        // ----------------- Prepositions -----------------
        AddWord(new Word("to", PartsOfSpeech.Preposition));

        // ----------------- Multi-role word example: run -----------------
        var runNoun = new NounForms { Singular = "run", Plural = "runs" };
        var runVerb = new VerbForms
        {
            Base = "run",
            Past = "ran",
            PastParticiple = "run",
            PresentParticiple = "running",
            ThirdPerson = "runs"
        };
        var runWord = new Word("run", PartsOfSpeech.Noun | PartsOfSpeech.Verb);
        runWord.AddNounForm(runNoun);
        runWord.AddVerbForm(runVerb);
        AddWord(runWord);
    }

    private static void AddWord(Word word)
    {
        void AddKey(string key)
        {
            key = key.ToLower();
            if (!Words.ContainsKey(key))
            {
                Words[key] = word;
            }
            else
            {
                // Merge forms if already exists
                var existing = Words[key];
                existing.PartOfSpeech |= word.PartOfSpeech;
                existing.NounFormsList.AddRange(word.NounFormsList);
                existing.VerbFormsList.AddRange(word.VerbFormsList);
            }
        }

        // Add base form
        AddKey(word.Text);

        // Index all verb forms safely
        foreach (var vf in new List<VerbForms>(word.VerbFormsList))
        {
            if (!string.IsNullOrEmpty(vf.Past)) AddKey(vf.Past);
            if (!string.IsNullOrEmpty(vf.PastParticiple)) AddKey(vf.PastParticiple);
            if (!string.IsNullOrEmpty(vf.PresentParticiple)) AddKey(vf.PresentParticiple);
            if (!string.IsNullOrEmpty(vf.ThirdPerson)) AddKey(vf.ThirdPerson);
            if (!string.IsNullOrEmpty(vf.FirstPersonSingular)) AddKey(vf.FirstPersonSingular);
            if (!string.IsNullOrEmpty(vf.SecondPersonSingular)) AddKey(vf.SecondPersonSingular);
            if (!string.IsNullOrEmpty(vf.ThirdPersonSingular)) AddKey(vf.ThirdPersonSingular);
            if (!string.IsNullOrEmpty(vf.FirstPersonPlural)) AddKey(vf.FirstPersonPlural);
            if (!string.IsNullOrEmpty(vf.SecondPersonPlural)) AddKey(vf.SecondPersonPlural);
            if (!string.IsNullOrEmpty(vf.ThirdPersonPlural)) AddKey(vf.ThirdPersonPlural);
        }

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
        var results = new List<ParsedWord>();
        if (string.IsNullOrEmpty(sentence)) return results;

        char[] delimiters = { ' ', ',', '.', ';', '!', '?', ':', '-' };
        string[] words = sentence.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

        int state = 0; // 0=subject,1=verb,2=object/modifier
        PartsOfSpeech subjectPOS = PartsOfSpeech.None;

        foreach (var rawWord in words)
        {
            string key = rawWord.ToLower();
            if (!WordDatabase.Words.TryGetValue(key, out Word word))
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



