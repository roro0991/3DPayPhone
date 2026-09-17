using Game.Info;
using System.Collections.Generic;
using UnityEngine;
using Game.World;

public class NarrativeRegistry
{
    private List<Information> Information_List = new();

    public void Register(Information info)
    {
        Information_List.Add(info);
    }

    public Information Find(Entity subject, Relationship relationship)
        // This method needs to be edited to account for multiple info
        // entries that have the same subject and relationship parameters
    {
        foreach (Information info in Information_List)
        {
            if (info.Subject == subject &&
                info.Relationship == relationship)
            {
                return info;
            }            
        }

        return null;
    }

    public Information FindBelief(Entity subject, Entity target, Relationship relationship)
    {
        foreach (Information belief in Information_List)
        {
            if (belief.Subject == subject &&
                belief.Relationship == Relationship.Believes &&
                belief.Object is Information info && 
                info.Subject == target &&
                info.Relationship == relationship)
            {
                Debug.Log("Belief found");
                return belief;
            }
        }

        Debug.Log("No belief found!");
        return null;
    }
}
