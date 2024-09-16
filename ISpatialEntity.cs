using UnityEngine;

public interface ISpatialEntity
{
    public SpatialCell CurrentCell { get; set; }
    public Vector3 GetPosition();
    public Transform GetTransform();
}