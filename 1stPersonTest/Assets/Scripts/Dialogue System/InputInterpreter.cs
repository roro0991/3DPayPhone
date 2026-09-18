using Ink.Parsed;
using UnityEngine;
using System.Collections.Generic;
using Dialogue.Core;
using Game.World;
using Game.Info;

public class InterpretedInputData
{
    public InputMode InputMode;
    public QueryMode QueryMode;        
    public Entity Subject;
    public Entity Object;
    public Information Topic;
    public SentenceWordEntry Verb;
}

public class InputInterpreter : MonoBehaviour
{

    public InterpretedInputData InterpretPlayerInput(WorkingInputData workingInputData)
    {
        var workingData = workingInputData;

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
        
        InterpretedInputData interpretedInputData = new InterpretedInputData();

        if (workingData.InputMode == InputMode.Statement)
        {
            interpretedInputData.InputMode = InputMode.Statement;
            interpretedInputData.QueryMode = QueryMode.None;
            interpretedInputData.Subject = WorldRegistryBootStrapper.World.Get(workingData.Subject.Word.EntityID);
            interpretedInputData.Object = WorldRegistryBootStrapper.World.Get(workingData.Object.Word.EntityID);
            interpretedInputData.Verb = workingData.Verb;
        }

        if (workingData.InputMode == InputMode.Query)
        {
            QueryMode queryMode = workingData.QueryMode;
            Debug.Log("subject: " + workingData.Subject.Word.Text);
            Debug.Log("object: " + workingData.Object.Word.Text);
            Debug.Log("verb: " + workingData.Verb.Word.Text);

            switch (queryMode)
            {
                case QueryMode.Int_What:
                    interpretedInputData.QueryMode = workingData.QueryMode;
                    interpretedInputData.Subject = WorldRegistryBootStrapper.World.Get(workingData.Subject.Word.EntityID);
                    if (workingData.Object.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
                    {
                        interpretedInputData.Object = null;                    
                    }
                    else
                    {
                        interpretedInputData.Object = WorldRegistryBootStrapper.World.Get(workingData.Object.Word.Text);                    
                    }
                    interpretedInputData.Verb = workingData.Verb;
                    break;
                case QueryMode.Int_Where:
                    interpretedInputData.QueryMode = workingData.QueryMode;
                    interpretedInputData.Subject = WorldRegistryBootStrapper.World.Get(workingData.Subject.Word.EntityID);
                    //interpretedQuery.Target = WorldRegistryBootStrapper.World.Get(workingData.Object.Word.Text);
                    interpretedInputData.Verb = workingData.Verb;
                    break;
                case QueryMode.Polar_Do:
                    interpretedInputData.QueryMode = workingData.QueryMode;
                    interpretedInputData.Subject = WorldRegistryBootStrapper.World.Get(workingData.Subject.Word.EntityID);
                    interpretedInputData.Object = WorldRegistryBootStrapper.World.Get(workingData.Object.Word.EntityID);
                    interpretedInputData.Verb = workingData.Verb;
                    break;
                default:
                    break;
            }
        }

        
        // Statement logic will go here. workingInputData will include
        // a reference to current entity target so they player can ask
        // follow-up questions. For example, if the player asked
        // "Did you lie about your identity?" and the NPC responds in
        // the negative, the player can simply respond with "I don't
        // believe you". 

        // Similar logic should be added to queries allowing the player
        // to ask follow up questions. For example, if the player asks
        // "Did you lie about your identity?" and the nPC responds in the
        // affirmative, the player can simply ask, "Why?"
        

        Debug.Log("Interpreted query subject id: " + interpretedInputData.Subject.Id);
        Debug.Log("interpreted query verb: " + interpretedInputData.Verb.Surface);

        return interpretedInputData;
    }

}
