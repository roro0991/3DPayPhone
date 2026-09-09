using UnityEngine;

namespace Dialogue.Core
{
    public enum QueryWord
    {
        Who,
        What,
        Where,
        When,
        Why,
        Is,
        Do,
    }

    public class PlayerQuestionData
    {
        public QueryWord QueryWord;
        public bool isCopular;

        public SentenceWordEntry Subject;
        public SentenceWordEntry Object;
        public SentenceWordEntry Verb;
    }

}
