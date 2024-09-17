using UnityEngine;

public interface ISpatialEntity
{
    /// <summary>
    /// The cell this entity is currently in.
    /// </summary>
    public SpatialCell CurrentCell { get; set; }
    /// <summary>
    /// The position used by the grid to determines in which cell this entity is.
    /// </summary>
    /// <returns>The position to use.</returns>
    public Vector3 GetPosition();
    /// <summary>
    /// Implement this method to customize the name that will be displayed in the debugEntitiesDisplay list of each cell. It should be unique.
    /// </summary>
    /// <returns>The name to use.</returns>
    public string GetDebugName();
}