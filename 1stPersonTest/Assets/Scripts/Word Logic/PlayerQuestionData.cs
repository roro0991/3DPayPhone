using UnityEngine;

namespace Dialogue.Core
{
    public enum InterrogativeType
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
        public InterrogativeType Interrogative;
        public bool isCopular;

        public SentenceWordEntry Subject;
        public SentenceWordEntry Object;
        public SentenceWordEntry Verb;
    }

}
