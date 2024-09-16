using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGridTurret : MonoBehaviour
{
    [Min(1f)][SerializeField] private float range = 5f;
    [SerializeField] private SpatialGrid grid;

    private List<SpatialCell> _cellsInRange = new();

    private void Start()
    {
        _cellsInRange = grid.GetCellsInRangeFromPoint(transform.position, range);
    }

    private void OnDrawGizmosSelected()
    {
        if (!grid)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        
        Gizmos.color = Color.green;
        float halfCellSize = grid.CellSize / 2f;
        foreach (var cell in _cellsInRange)
        {
            Vector3 center = grid.Axis switch
            {
                SpatialGridAxis.XZ => new Vector3(cell.Coordinates.x + halfCellSize, 0f, cell.Coordinates.y + halfCellSize),
                SpatialGridAxis.XY => new Vector3(cell.Coordinates.x + halfCellSize, cell.Coordinates.y + halfCellSize, 0f),
                SpatialGridAxis.YZ => new Vector3(0f, cell.Coordinates.x + halfCellSize, cell.Coordinates.y + halfCellSize),
                _ => throw new ArgumentOutOfRangeException()
            };
            Gizmos.DrawSphere(center, cell.Size / 3f);
        }
    }
}