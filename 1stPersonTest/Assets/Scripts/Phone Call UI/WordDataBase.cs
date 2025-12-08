using System.Collections.Generic;
using UnityEngine;

public class WordDataBase : MonoBehaviour
{
    public static WordDataBase Instance { get; private set; }

    private Dictionary<string, Word> _words = new Dictionary<string, Word>();
    public IReadOnlyDictionary<string, Word> Words => _words;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optional: only if this should persist across scenes
        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);

        PopulateDictionary();
        Debug.Log($"WordDatabase initialized. Total words: {_words.Count}");
    }


    private void PopulateDictionary()
    {
        _words.Clear();

        // ----------------- Interrogatives -----------------
        AddWord(new Word(".", PartsOfSpeech.Punctuation));
        AddWord(new Word("?", PartsOfSpeech.Punctuation));

        // ----------------- Conjunctions -----------------
        AddWord(new Word("and", PartsOfSpeech.Conjunction));

        // ----------------- Interrogatives -----------------
        AddWord(new Word("who", PartsOfSpeech.Interrogative));
        AddWord(new Word("what", PartsOfSpeech.Interrogative));
        AddWord(new Word("where", PartsOfSpeech.Interrogative));
        AddWord(new Word("when", PartsOfSpeech.Interrogative));
        AddWord(new Word("why", PartsOfSpeech.Interrogative));

        // ----------------- Articles -----------------
        AddWord(new Word("the", PartsOfSpeech.Article));
        AddWord(new Word("a", PartsOfSpeech.Article));
        AddWord(new Word("an", PartsOfSpeech.Article));

        // ----------------- Interjections -----------------
        AddWord(new Word("hi", PartsOfSpeech.Interjection));

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

        // ----------------- Nouns -----------------
        var dogForms = new NounForms { Singular = "dog", Plural = "dogs" };
        var dogWord = new Word("dog", PartsOfSpeech.Noun);
        dogWord.AddNounForm(dogForms);
        AddWord(dogWord);

        var catForms = new NounForms { Singular = "cat", Plural = "cats" };
        var catWord = new Word("cat", PartsOfSpeech.Noun);
        catWord.AddNounForm(catForms);
        AddWord(catWord);

        var elephantForms = new NounForms { Singular = "elephant", Plural = "elephants" };
        var elephantWord = new Word("elephant", PartsOfSpeech.Noun);
        elephantWord.AddNounForm(elephantForms);
        AddWord(elephantWord);

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

        // ----------------- Adverb -----------------

        AddWord(new Word("there", PartsOfSpeech.Adverb | PartsOfSpeech.SubjectPronoun | PartsOfSpeech.Interjection));

        // ----------------- Adjectives -----------------
        AddWord(new Word("nice", PartsOfSpeech.Adjective));
        AddWord(new Word("big", PartsOfSpeech.Adjective));

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

        // ----------------- Test Words -----------------
        AddWord(new Word("hello", PartsOfSpeech.Interjection));
    }

    private void AddWord(Word word)
    {
        void AddKey(string key)
        {
            key = key.ToLower();
            if (!_words.ContainsKey(key))
            {
                _words[key] = word;
            }
            else
            {
                var existing = _words[key];
                existing.PartOfSpeech |= word.PartOfSpeech;
                existing.NounFormsList.AddRange(word.NounFormsList);
                existing.VerbFormsList.AddRange(word.VerbFormsList);
            }
        }

        AddKey(word.Text);

        // Safe enumeration of verb forms
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

    public Word GetWord(string key)
    {
        _words.TryGetValue(key.ToLower(), out var word);

        if (word == null)
        {
            foreach (var w in WordDataBase.Instance.Words.Values)
            {
                foreach (var nf in w.NounFormsList)
                {
                    if (nf.Plural == key)
                    {
                        word = w;
                        break;
                    }
                }
            }
        }
        return word;
    }

    public bool IsWordReady(string key)
    {
        return _words.ContainsKey(key.ToLower());
    }
}


