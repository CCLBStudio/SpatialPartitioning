using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpatialCell
{
    #if UNITY_EDITOR
    [SerializeField] private List<string> debugEntitiesDisplay = new();
    private Dictionary<ISpatialEntity, string> _entityNamesHash = new();
    #endif
    
    public readonly List<ISpatialEntity> entities = new();
    public Vector2Int Coordinates { get; private set; }
    public int Size { get; private set; }
    public SpatialGrid Grid { get; private set; }

    public SpatialCell(Vector2Int coordinates, SpatialGrid grid)
    {
        Coordinates = coordinates;
        Size = grid.CellSize;
        Grid = grid;
    }

    public void AddEntity(ISpatialEntity entity)
    {
        if (entity.CurrentCell == this)
        {
            return;
        }

        entity.CurrentCell?.RemoveEntity(entity);
        entities.Add(entity);
        entity.CurrentCell = this;
        
#if UNITY_EDITOR
        if (!Grid.requireDebugDataDisplay)
        {
            return;
        }
        _entityNamesHash.Add(entity, entity.GetDebugName());
        debugEntitiesDisplay = new List<string>(_entityNamesHash.Values);
#endif
    }
    
    public void RemoveEntity(ISpatialEntity entity)
    {
        if (entity.CurrentCell != this)
        {
            return;
        }
        
        entities.Remove(entity);
        entity.CurrentCell = null;
        
#if UNITY_EDITOR
        if (!Grid.requireDebugDataDisplay)
        {
            return;
        }
        _entityNamesHash.Remove(entity);
        debugEntitiesDisplay = new List<string>(_entityNamesHash.Values);
#endif
    }
}