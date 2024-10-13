using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "CCLB Studio/Spatial Partitioning/Spatial Grid")]
public class SpatialGrid : ScriptableObject
{
    public SpatialGridAxis Axis => axis;
    public int CellSize => cellSize;
    public List<SpatialCell> Cells => _grid.Values.ToList();
    public bool AutomaticCheckForNullEntities => automaticCheckForNullEntities;

    [Tooltip("The grid axis, determining how the spatial checks will be performed.")]
    [SerializeField] private SpatialGridAxis axis = SpatialGridAxis.XZ;
    [Tooltip("The size of each cell of the grid.")]
    [Range(1, 20)][SerializeField] private int cellSize = 1;
    [Tooltip("If TRUE, the grid will initialize itself during the OnEnable method.")]
    [SerializeField] private bool autoInitialize = true;
    [Tooltip("If TRUE, everytime you ask for the entities inside a cell it will first perform a check to remove any null entity. Less performant but simpler to use. For better performance, every entity should remove itself from its cell before being destroyed.")]
    [SerializeField] private bool automaticCheckForNullEntities = true;

    [NonSerialized] private readonly Dictionary<Vector2Int, SpatialCell> _grid = new();
    private Func<Vector3, Vector2Int> _getCoordinates;
    private Func<Vector3, Vector3> _clampPointToCoordinates;
    private Func<Vector3, SpatialCell, Vector3> _getClampedPointForCellCheck;
    [NonSerialized] private bool _isInit;
    
    #if UNITY_EDITOR
    [Header("Debug Visualisation (Editor Only)")]
    [Tooltip("If TRUE, the debug grid will be populated and the cells will update their debug entity list.")]
    public bool requireDebugDataDisplay = true;
    [Tooltip("The debug grid, to help you visualize the data inside your grid.")]
    [SerializeField] private List<SpatialCell> debugGrid;
#endif

    #region Unity Events

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            Initialize();
            return;
        }
        
        if (!autoInitialize)
        {
            return;
        }
        
        Initialize();
    }

    private void OnDisable()
    {
        ClearGrid();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ClearGrid();
            ComputeCoordinatesDelegate();
            ComputeClampPointForRangeCheck();
            ComputeClampPointToCoordinatesDelegate();
        }
    }
#endif

    #endregion
    
    #region Initialization

    public void Initialize()
    {
        if (_isInit)
        {
            return;
        }

        _isInit = true;
        ComputeCoordinatesDelegate();
        ComputeClampPointForRangeCheck();
        ComputeClampPointToCoordinatesDelegate();
    }

    private void ComputeCoordinatesDelegate()
    {
        switch (axis)
        {
            case SpatialGridAxis.XZ:
                _getCoordinates = point => new Vector2Int(
                    Mathf.FloorToInt(point.x / cellSize) * cellSize,
                    Mathf.FloorToInt(point.z / cellSize) * cellSize);
                break;
            
            case SpatialGridAxis.XY:
                _getCoordinates = point => new Vector2Int(
                    Mathf.FloorToInt(point.x / cellSize) * cellSize,
                    Mathf.FloorToInt(point.y / cellSize) * cellSize);
                break;
            
            case SpatialGridAxis.YZ:
                _getCoordinates = point => new Vector2Int(
                    Mathf.FloorToInt(point.y / cellSize) * cellSize,
                    Mathf.FloorToInt(point.z / cellSize) * cellSize);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ComputeClampPointToCoordinatesDelegate()
    {
        switch (axis)
        {
            case SpatialGridAxis.XZ:
                _clampPointToCoordinates = point => new Vector3(Mathf.Floor(point.x / cellSize) * cellSize, point.y, Mathf.Floor(point.z / cellSize) * cellSize);
                break;
            
            case SpatialGridAxis.XY:
                _clampPointToCoordinates = point => new Vector3(Mathf.Floor(point.x / cellSize) * cellSize, Mathf.Floor(point.y / cellSize) * cellSize, point.z);
                break;
            
            case SpatialGridAxis.YZ:
                _clampPointToCoordinates = point => new Vector3(point.x, Mathf.Floor(point.y / cellSize) * cellSize, Mathf.Floor(point.z / cellSize) * cellSize);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void ComputeClampPointForRangeCheck()
    {
        switch (axis)
        {
            case SpatialGridAxis.XZ:
                _getClampedPointForCellCheck = (point, cell) => new Vector3(Mathf.Max(cell.Min.x, Mathf.Min(point.x, cell.Max.x)), point.y, Mathf.Max(cell.Min.z, Mathf.Min(point.z, cell.Max.z)));
                break;
            
            case SpatialGridAxis.XY:
                _getClampedPointForCellCheck = (point, cell) => new Vector3(Mathf.Max(cell.Min.x, Mathf.Min(point.x, cell.Max.x)), Mathf.Max(cell.Min.y, Mathf.Min(point.y, cell.Max.y)), point.z);
                break;
            
            case SpatialGridAxis.YZ:
                _getClampedPointForCellCheck = (point, cell) => new Vector3(point.x, Mathf.Max(cell.Min.y, Mathf.Min(point.y, cell.Max.y)), Mathf.Max(cell.Min.z, Mathf.Min(point.z, cell.Max.z)));
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion
    
    #region Grid Methods

    public List<SpatialCell> GetCellsInRangeFromPoint(Vector3 point, float range)
    {
        int cellRange = Mathf.CeilToInt(range / cellSize);
        Vector2Int centerCell = GetCellCoordinate(point);
        List<SpatialCell> inRange = new List<SpatialCell>();

        for (int i = -cellRange; i <= cellRange; i++)
        {
            for (int j = -cellRange; j <= cellRange; j++)
            {
                Vector2Int coordinate = new Vector2Int(centerCell.x + i, centerCell.y + j);
                var cell = GetOrCreateCell(coordinate);
                Vector3 clampedPoint = GetClampedPointForCellCheck(point, cell);
                float distance = Vector3.Distance(clampedPoint, point);
                
                if (distance <= range)
                {
                    inRange.Add(cell);
                }
            }
        }

        return inRange;
    }

    public Vector2Int GetCellCoordinate(Vector3 point)
    {
        return _getCoordinates(point);
    }

    public Vector3 ClampPointToCoordinates(Vector3 point)
    {
        return _clampPointToCoordinates(point);
    }

    private Vector3 GetClampedPointForCellCheck(Vector3 point, SpatialCell cell)
    {
        return _getClampedPointForCellCheck(point, cell);
    }

    public void ClearGrid()
    {
        _grid.Clear();
#if UNITY_EDITOR
        debugGrid.Clear();
#endif
    }

    public void RefreshEntityPosition(ISpatialEntity entity)
    {
        Vector2Int cellCoord = GetCellCoordinate(entity.GetPosition());
        var cell = GetOrCreateCell(cellCoord);
        cell.AddEntity(entity);
    }

    public void RemoveEntity(ISpatialEntity entity)
    {
        Vector2Int cellCoord = GetCellCoordinate(entity.GetPosition());
        if (!_grid.ContainsKey(cellCoord))
        {
            return;
        }
        
        var cell = _grid[cellCoord];
        cell.RemoveEntity(entity);
    }

    private SpatialCell GetOrCreateCell(Vector2Int coordinates)
    {
        if (_grid.TryGetValue(coordinates, out var needed))
        {
            return needed;
        }

        _grid.Add(coordinates, new SpatialCell(coordinates, this));
        var cell = _grid[coordinates];

#if UNITY_EDITOR
        if (requireDebugDataDisplay)
        {
            debugGrid.Add(cell);
        }
#endif

        return cell;
    }

    #endregion
}