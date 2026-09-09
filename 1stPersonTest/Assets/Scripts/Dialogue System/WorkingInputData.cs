using UnityEngine;

namespace Dialogue.Core
{
    public enum QueryMode
    {
        None,
        Int_Who,
        Int_What,
        Int_Where,
        Int_When,
        Int_Why,
        Copular_Be,
        Polar_Do,
    }

    public enum InputMode
    {
        Query,
        Statement
    }

    public class WorkingInputData
    {
        public InputMode InputMode;
        public QueryMode QueryMode;

        public SentenceWordEntry Subject;
        public SentenceWordEntry Object;
        public SentenceWordEntry Verb;
    }

}
