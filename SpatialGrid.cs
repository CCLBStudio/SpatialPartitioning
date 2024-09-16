using System;
using System.Collections.Generic;
using UnityEngine;

public class SpatialGrid : MonoBehaviour
{
    public SpatialGridAxis Axis => axis;
    public int CellSize => cellSize;

    [SerializeField] private SpatialGridAxis axis = SpatialGridAxis.XZ;
    [Range(1, 20)][SerializeField] private int cellSize = 1;
    [SerializeField] private bool autoInitialize = true;

    private readonly Dictionary<Vector2Int, SpatialCell> _grid = new();

    private Func<Vector3, Vector2Int> _getCoordinates;
    private bool _isInit;

    #region Editor Variables
#if UNITY_EDITOR
    [Header("Gizmos (Editor Only)")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool showExistingCells = true;
    [Min(5)][SerializeField] private int gizmoGridSize = 50;
    [Range(0f, 180f)] [SerializeField] private float arrowAngle = 45f;
    [Range(0f, 1f)] [SerializeField] private float arrowSize = .5f;

    [HideInInspector][SerializeField] private Vector3 normalizedSideAxis = Vector3.right;
    [HideInInspector][SerializeField] private Vector3 normalizedForwardAxis = Vector3.forward;
    [HideInInspector][SerializeField] private Vector3 normalizedOffsetAxis = new Vector3(1f, 0f, 1f);
#endif
    #endregion

    #region Unity Events

    private void Awake()
    {
        if (!autoInitialize)
        {
            return;
        }
        Initialize();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }
        
        int halfSize = Mathf.FloorToInt(gizmoGridSize / 2f);
        int actualSize = halfSize * 2;

        Vector3 position = transform.position;
        Vector3 startPos = position - halfSize * (normalizedOffsetAxis * cellSize);
        Vector3 sideAxis = normalizedSideAxis * cellSize;
        Vector3 forwardAxis = normalizedForwardAxis * cellSize;

        for (int i = 1; i < actualSize; i++)
        {
            Vector3 from = startPos + i * sideAxis;
            Vector3 to = from + actualSize * forwardAxis;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(from, to);
            DrawArrow(from, to);
            DrawArrow(to, from);

            from = startPos + i * forwardAxis;
            to = from + actualSize * sideAxis;

            Gizmos.DrawLine(from, to);
            DrawArrow(from, to);
            DrawArrow(to, from);
        }

        if (showExistingCells)
        {
            Gizmos.color = Color.red;
            foreach (var cell in _grid.Values)
            {
                Gizmos.DrawSphere(cell.Center, cellSize / 5f);
            }
        }
    }
    
    private void OnValidate()
    {
        switch (axis)
        {
            case SpatialGridAxis.XZ:
                normalizedSideAxis = Vector3.right;
                normalizedForwardAxis = Vector3.forward;
                normalizedOffsetAxis = new Vector3(1f, 0f, 1f);
                break;
            
            case SpatialGridAxis.XY:
                normalizedSideAxis = Vector3.up;
                normalizedForwardAxis = Vector3.right;
                normalizedOffsetAxis = new Vector3(1f, 1f, 0f);
                break;
            
            case SpatialGridAxis.YZ:
                normalizedSideAxis = Vector3.forward;
                normalizedForwardAxis = Vector3.up;
                normalizedOffsetAxis = new Vector3(0f, 1f, 1f);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }

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
    }

    public void RefreshEntity(ISpatialEntity entity)
    {
        Vector2Int cellCoord = GetCellCoordinate(entity.GetPosition());

        _grid.TryAdd(cellCoord, new SpatialCell(cellCoord, this));
        var cell = _grid[cellCoord];
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

    #endregion

    
    #region Editor Methods
#if UNITY_EDITOR
    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 up = Vector3.Cross(normalizedSideAxis, normalizedForwardAxis);

        Vector3 leftArrow = Quaternion.AngleAxis(arrowAngle, up) * -direction;
        Vector3 rightArrow = Quaternion.AngleAxis(-arrowAngle, up) * -direction;
        
        Gizmos.DrawLine(to, to + leftArrow * (arrowSize * cellSize));
        Gizmos.DrawLine(to, to + rightArrow * (arrowSize * cellSize));
    }
#endif
    #endregion
}