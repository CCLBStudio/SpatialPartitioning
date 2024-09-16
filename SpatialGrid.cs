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

    [SerializeField] private SpatialGridAxis axis = SpatialGridAxis.XZ;
    [Range(1, 20)][SerializeField] private int cellSize = 1;
    [SerializeField] private bool autoInitialize = true;

    private readonly Dictionary<Vector2Int, SpatialCell> _grid = new();
    private Func<Vector3, Vector2Int> _getCoordinates;
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

    #endregion
    
    #region Grid Methods

    public Vector2Int GetCellCoordinate(Vector3 point)
    {
        return _getCoordinates(point);
    }

    public void ClearGrid()
    {
        _grid.Clear();
#if UNITY_EDITOR
        debugGrid.Clear();
#endif
    }

    public void RefreshEntity(ISpatialEntity entity)
    {
        Vector2Int cellCoord = GetCellCoordinate(entity.GetPosition());

        bool added = _grid.TryAdd(cellCoord, new SpatialCell(cellCoord, this));
        var cell = _grid[cellCoord];
        cell.AddEntity(entity);

#if UNITY_EDITOR
        if (added && requireDebugDataDisplay)
        {
            debugGrid.Add(cell);
        }
#endif
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

    #endregion
}