using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpatialCell
{
    #if UNITY_EDITOR
    [SerializeField] private List<Transform> debug = new();
    #endif
    
    public readonly List<ISpatialEntity> entities = new();
    public Vector2Int Coordinates { get; private set; }
    public Vector3 Center { get; private set; }
    public int Size { get; private set; }

    public SpatialCell(Vector2Int coordinates, SpatialGrid grid)
    {
        Coordinates = coordinates;
        Size = grid.CellSize;
        
        Vector3 gridPos = grid.transform.position;
        float halfSize = Size / 2f;
        Center = grid.Axis switch
        {
            SpatialGridAxis.XZ => new Vector3(coordinates.x + halfSize, gridPos.y, coordinates.y + halfSize),
            SpatialGridAxis.XY => new Vector3(coordinates.x + halfSize, coordinates.y + halfSize, gridPos.z),
            SpatialGridAxis.YZ => new Vector3(gridPos.x, coordinates.x + halfSize, coordinates.y + halfSize),
            _ => throw new ArgumentOutOfRangeException()
        };
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
        debug.Add(entity.GetTransform());
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
        debug.Remove(entity.GetTransform());
#endif
    }
}