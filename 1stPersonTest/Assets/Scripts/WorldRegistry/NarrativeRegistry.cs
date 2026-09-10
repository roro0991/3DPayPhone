using Game.Facts;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeRegistry
{
    private Dictionary<string, Topic> facts = new();

    public void Register(Topic fact)
    {
        facts[fact.FactID] = fact;
    }

    public Topic Get(string factID)
    {
        return facts.TryGetValue(factID, out var fact) ? fact : null;
    }
}
