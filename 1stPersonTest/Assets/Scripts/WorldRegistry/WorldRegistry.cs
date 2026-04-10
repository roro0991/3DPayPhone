using UnityEngine;
using Game.World;
using System.Collections.Generic;

public class WorldRegistry
{
    private Dictionary<string, Entity> entities = new();

    public void Register(Entity entity)
    {
        entities[entity.Id] = entity;
    }

    public Entity Get(string Id)
    {
        return entities.TryGetValue(Id, out var entity) ? entity : null;
    }


}
