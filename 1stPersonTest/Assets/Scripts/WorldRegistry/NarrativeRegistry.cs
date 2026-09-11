using Game.Facts;
using System.Collections.Generic;
using UnityEngine;
using Game.World;

public class NarrativeRegistry
{
    private Dictionary<string, Information> information = new();

    public void Register(Information info)
    {
        information[info.InfoID] = info;
    }

    public Information Get(string infoID)
    {
        return information.TryGetValue(infoID, out var fact) ? fact : null;
    }

    public Information Find(Entity subject, Relationship relationship)
    {
        foreach (Information info in information.Values)
        {
            if (info.Subject == subject &&
                info.Relationship == relationship)
            {
                return info;
            }
        }

        return null;
    }
}
