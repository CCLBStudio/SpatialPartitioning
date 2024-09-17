using System;
using System.Collections.Generic;
using System.Linq;
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
    public Vector3 Center { get; private set; }
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }
    public int Size { get; private set; }
    public SpatialGrid Grid { get; private set; }

    public SpatialCell(Vector2Int coordinates, SpatialGrid grid)
    {
        Coordinates = coordinates;
        Size = grid.CellSize;
        Grid = grid;
        float halfSize = Size / 2f;

        switch (grid.Axis)
        {
            case SpatialGridAxis.XZ:
                Min = new Vector3(coordinates.x, 0f, coordinates.y);
                Max = new Vector3(coordinates.x + Size, 0f, coordinates.y + Size);
                Center = new Vector3(coordinates.x + halfSize, 0f, coordinates.y + halfSize);
                break;
            
            case SpatialGridAxis.XY:
                Min = new Vector3(coordinates.x, coordinates.y, 0f);
                Max = new Vector3(coordinates.x + Size, coordinates.y + Size, 0f);
                Center = new Vector3(coordinates.x + halfSize, coordinates.y + halfSize, 0f);
                break;
            
            case SpatialGridAxis.YZ:
                Min = new Vector3(0f, coordinates.x, coordinates.y);
                Max = new Vector3(0f, coordinates.x + Size, coordinates.y + Size);
                Center = new Vector3(0f, coordinates.x + halfSize, coordinates.y + halfSize);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public IEnumerable<T> GetEntitiesOfType<T>() where T : ISpatialEntity
    {
        return entities.OfType<T>();
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