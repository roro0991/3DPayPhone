using Game.Facts;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeRegistry
{
    private Dictionary<string, Fact> facts = new();

    public void Register(Fact fact)
    {
        facts[fact.FactID] = fact;
    }

    public Fact Get(string factID)
    {
        return facts.TryGetValue(factID, out var fact) ? fact : null;
    }
}
