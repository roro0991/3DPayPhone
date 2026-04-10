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

        if (workingData == null)
        {
            Debug.LogError("workingData is NULL");
            return null;
        }

        if (workingData.Subject == null)
        {
            Debug.LogError("workingData.Subject is NULL");
            return null;
        }

        if (workingData.Subject.Word == null)
        {
            Debug.LogError("workingData.Subject.Word is NULL");
            return null;
        }

        if (workingData.Subject.Word.EntityID == null)
        {
            Debug.LogError("workingData.Subject.Word.Entity is NULL");
            return null;
        }

        InterrogativeType interrogative = workingData.Interrogative;
            
        InterpretedQuery interpretedQuery = new InterpretedQuery();

        switch (interrogative)
        {
            case InterrogativeType.What:
                interpretedQuery.Interrogative = workingData.Interrogative;
                interpretedQuery.Subject = WorldRegistryBootStrapper.World.Get(workingData.Subject.Word.EntityID);
                interpretedQuery.Target = WorldRegistryBootStrapper.World.Get(workingData.Object.Word.Text);
                break;
            default:
                break;
        }

        Debug.Log("Interpreted query subject id: " + interpretedQuery.Subject.Id);

        return interpretedQuery;
    }

}
