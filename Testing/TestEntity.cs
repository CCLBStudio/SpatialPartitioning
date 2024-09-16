using System;
using UnityEngine;

public class TestEntity : MonoBehaviour, ISpatialEntity
{
    public SpatialCell CurrentCell { get; set; }

    [SerializeField] private SpatialGrid grid;

    private void Update()
    {
        grid.RefreshEntity(this);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public string GetDebugName()
    {
        return name;
    }
}