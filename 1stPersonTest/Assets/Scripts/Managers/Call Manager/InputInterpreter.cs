using Ink.Parsed;
using UnityEngine;
using System.Collections.Generic;
using Dialogue.Core;
using Game.World;

public class InterpretedQuery
{
    public InterrogativeType Interrogative;        
    public Entity Subject;
    public Entity Target;
    public SentenceWordEntry Verb;
}

public class InputInterpreter : MonoBehaviour
{

    public InterpretedQuery InterpretPlayerInput(PlayerQuestionData questionData)
    {
        var workingData = questionData;

        InterrogativeType interrogative = workingData.Interrogative;
            
        InterpretedQuery interpretedQuery = new InterpretedQuery();

        switch (interrogative)
        {
            case InterrogativeType.What:
                interpretedQuery.Interrogative = workingData.Interrogative;
                interpretedQuery.Subject = workingData.Subject.Word.Entity;
                interpretedQuery.Target = WorldRegistryBootStrapper.World.Get(workingData.Object.Word.Text);
                break;
            default:
                break;
        }

        return interpretedQuery;
    }

}
