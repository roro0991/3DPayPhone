using System.Collections.Generic;
using UnityEngine;

namespace Dialogue.Core
{
    [System.Serializable]
    public class SentenceWordEntry
    {
        public Word Word; // semantic object
        public string Surface; // the actual form used (ie. dog vs dogs)
        public DraggableWord.DraggableOrigin Origin;
        public PartsOfSpeech activePOS;

        // Relationship references
        [System.NonSerialized]
        public SentenceWordEntry owningSubject; // verb relationship
        public SentenceWordEntry owningNoun; // corresponding noun to this article
        public SentenceWordEntry owningVerb; // corresponding to this adverb
        public SentenceWordEntry owningAdjective; // corresponding to this adverb
        public SentenceWordEntry owningInterrogative; // corresponding to this auxiliary
        public SentenceWordEntry article; // this noun's article
        public SentenceWordEntry verb; // this noun's verb       
        public SentenceWordEntry auxiliary; // this verb's auxiliary
        public SentenceWordEntry preposition; // this word's preposition
        public SentenceWordEntry negation; // this verb's adverb of negation
        public Queue<SentenceWordEntry> adjectives = new();

        public bool isPreview = false;
        public bool isObject = false;
        public bool isSubject = false;
    }
}
