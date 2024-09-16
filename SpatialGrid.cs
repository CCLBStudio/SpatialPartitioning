using System;
using ReaaliStudio.Core.Utils;
using UnityEngine;

public class SpatialGrid : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Gizmos (Editor Only)")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool showExistingCells = true;
    [Min(5)][SerializeField] private int gizmoGridSize = 50;
#endif

    public SpatialGridAxis Axis => axis;
    public int CellSize => cellSize;

    [SerializeField] private SpatialGridAxis axis = SpatialGridAxis.XZ;
    [Range(1, 20)][SerializeField] private int cellSize = 1;

    [DisableGUI]
    [SerializeField] private SerializableDictionary<Vector2Int, SpatialCell> grid = new();

    public Vector2Int GetCellCoordinate(Vector3 point)
    {
        int x;
        int y;

        switch (axis)
        {
            case SpatialGridAxis.XZ:
                x = Mathf.FloorToInt(point.x / cellSize) * cellSize;
                y = Mathf.FloorToInt(point.z / cellSize) * cellSize;
                break;
            
            case SpatialGridAxis.XY:
                x = Mathf.FloorToInt(point.x / cellSize) * cellSize;
                y = Mathf.FloorToInt(point.y / cellSize) * cellSize;
                break;
            
            case SpatialGridAxis.YZ:
                x = Mathf.FloorToInt(point.y / cellSize) * cellSize;
                y = Mathf.FloorToInt(point.z / cellSize) * cellSize;
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return new Vector2Int(x, y);
    }

    public void RefreshEntity(ISpatialEntity entity)
    {
        Vector2Int cellCoord = GetCellCoordinate(entity.GetPosition());

        grid.Add(cellCoord, new SpatialCell(cellCoord, this));
        var cell = grid[cellCoord];
        cell.AddEntity(entity);
    }

    public void RemoveEntity(ISpatialEntity entity)
    {
        Vector2Int cellCoord = GetCellCoordinate(entity.GetPosition());
        if (!grid.ContainsKey(cellCoord))
        {
            return;
        }
        
        var cell = grid[cellCoord];
        cell.RemoveEntity(entity);
    }

    #region Gizmos
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }
        
        int halfSize = Mathf.FloorToInt(gizmoGridSize / 2f);
        int actualSize = halfSize * 2;
        
        Vector3 offset = axis switch
        {
            SpatialGridAxis.XZ => new Vector3(1f, 0f, 1f),
            SpatialGridAxis.XY => new Vector3(1f, 1f, 0f),
            SpatialGridAxis.YZ => new Vector3(0f, 1f, 1f)
        };

        Vector3 sideAxis = axis switch
        {
            SpatialGridAxis.XZ => Vector3.right * cellSize,
            SpatialGridAxis.XY => Vector3.right * cellSize,
            SpatialGridAxis.YZ => Vector3.up * cellSize
        };
        
        Vector3 forwardAxis = axis switch
        {
            SpatialGridAxis.XZ => Vector3.forward * cellSize,
            SpatialGridAxis.XY => Vector3.up * cellSize,
            SpatialGridAxis.YZ => Vector3.forward * cellSize
        };

        offset *= cellSize;
        var position = transform.position;
        Vector3 startPos = position - halfSize * offset;

        for (int i = 0; i <= actualSize; i++)
        {
            Vector3 from = startPos + i * sideAxis;
            Vector3 to = from + actualSize * forwardAxis;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(from, to);

            from = startPos + i * forwardAxis;
            to = from + actualSize * sideAxis;
            Gizmos.DrawLine(from, to);
        }

        if (showExistingCells)
        {
            Gizmos.color = Color.red;
            foreach (var cell in grid.GetValues())
            {
                Gizmos.DrawSphere(cell.Center, cellSize / 5f);
            }
        }
    }
#endif
    #endregion
}