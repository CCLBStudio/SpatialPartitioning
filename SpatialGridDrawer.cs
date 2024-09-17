using System;
using UnityEngine;

public class SpatialGridDrawer : MonoBehaviour
{
    [SerializeField] private SpatialGrid grid;
    
    #region Editor Variables
#if UNITY_EDITOR
    public static string GridProperty => nameof(grid);
    public static string DrawGizmosOnProperty => nameof(drawGizmosOn);
    public static string GridColorProperty => nameof(gridColor);
    public static string ShowExistingCellsProperty => nameof(showExistingCells);
    public static string ExistingCellsColorProperty => nameof(existingCellsColor);
    public static string GizmoGridSizeProperty => nameof(gizmoGridSize);
    public static string ArrowAngleProperty => nameof(arrowAngle);
    public static string ArrowSizeProperty => nameof(arrowSize);
    
    [Header("Gizmos (Editor Only)")]
    [SerializeField] private GizmosEvent drawGizmosOn = GizmosEvent.OnDrawGizmosSelected;
    [SerializeField] private Color gridColor = Color.cyan;
    [SerializeField] private bool showExistingCells = true;
    [SerializeField] private Color existingCellsColor = Color.red;
    [Min(5)][SerializeField] private int gizmoGridSize = 50;
    [Range(0f, 180f)] [SerializeField] private float arrowAngle = 45f;
    [Range(0f, 1f)] [SerializeField] private float arrowSize = .5f;

    [HideInInspector][SerializeField] private Vector3 normalizedSideAxis = Vector3.right;
    [HideInInspector][SerializeField] private Vector3 normalizedForwardAxis = Vector3.forward;
    [HideInInspector][SerializeField] private Vector3 normalizedOffsetAxis = new Vector3(1f, 0f, 1f);
    
    public enum GizmosEvent {None, OnDrawGizmos, OnDrawGizmosSelected}
#endif
    #endregion

    #region Unity Events
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (drawGizmosOn != GizmosEvent.OnDrawGizmos || !grid)
        {
            return;
        }
        
        DrawGrid();
    }

    private void OnDrawGizmosSelected()
    {
        if (drawGizmosOn != GizmosEvent.OnDrawGizmosSelected || !grid)
        {
            return;
        }
        
        DrawGrid();
    }

    private void OnValidate()
    {
        if (!grid)
        {
            return;
        }
        
        switch (grid.Axis)
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
    }
#endif

    #endregion
    #region Editor Methods
#if UNITY_EDITOR

    private void DrawGrid()
    {
        int halfSize = Mathf.FloorToInt(gizmoGridSize / 2f);
        int actualSize = halfSize * 2;

        Vector3 position = grid.ClampPointToCoordinates(transform.position);
        Vector3 startPos = position - halfSize * (normalizedOffsetAxis * grid.CellSize);
        Vector3 sideAxis = normalizedSideAxis * grid.CellSize;
        Vector3 forwardAxis = normalizedForwardAxis * grid.CellSize;

        for (int i = 1; i < actualSize; i++)
        {
            Vector3 from = startPos + i * sideAxis;
            Vector3 to = from + actualSize * forwardAxis;

            Gizmos.color = gridColor;
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
            Gizmos.color = existingCellsColor;
            Vector3 gridPos = transform.position;
            float halfCellSize = grid.CellSize / 2f;
            float sphereSize = grid.CellSize / 5f;

            foreach (var cell in grid.Cells)
            {
                Vector3 center = grid.Axis switch
                {
                    SpatialGridAxis.XZ => new Vector3(cell.Coordinates.x + halfCellSize, gridPos.y, cell.Coordinates.y + halfCellSize),
                    SpatialGridAxis.XY => new Vector3(cell.Coordinates.x + halfCellSize, cell.Coordinates.y + halfCellSize, gridPos.z),
                    SpatialGridAxis.YZ => new Vector3(gridPos.x, cell.Coordinates.x + halfCellSize, cell.Coordinates.y + halfCellSize),
                    _ => throw new ArgumentOutOfRangeException()
                };
                Gizmos.DrawSphere(center, sphereSize);
            }
        }
    }
    
    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 up = Vector3.Cross(normalizedSideAxis, normalizedForwardAxis);

        Vector3 leftArrow = Quaternion.AngleAxis(arrowAngle, up) * -direction;
        Vector3 rightArrow = Quaternion.AngleAxis(-arrowAngle, up) * -direction;
        
        Gizmos.DrawLine(to, to + leftArrow * (arrowSize * grid.CellSize));
        Gizmos.DrawLine(to, to + rightArrow * (arrowSize * grid.CellSize));
    }
#endif
    #endregion
}