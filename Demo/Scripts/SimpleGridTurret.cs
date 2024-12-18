using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.SpatialPartitioning;
using CCLBStudio.SpatialPartitioning.Grid;
using UnityEngine;

public class SimpleGridTurret : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private SpatialGrid grid;
    
    [Header("Turret Settings")]
    [Min(1f)][SerializeField] private float range = 5f;
    [SerializeField] private int damages = 20;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform lookAtPivot;
    [SerializeField] private Transform shootingPoint;

    private List<SpatialGridCell> _cellsInRange = new();
    private float _shootingTimer;
    private SimpleEnemy _currentTarget;

    private void Start()
    {
        _cellsInRange = grid.GetCellsInRangeFromPoint(transform.position, range);
    }

    private void Update()
    {
        if (_shootingTimer > 0f)
        {
            _shootingTimer -= Time.deltaTime * fireRate;
        }

        if (!_currentTarget)
        {
            SearchForEnemy();
            return;
        }

        if (Vector3.Distance(_currentTarget.GetPosition(), transform.position) > range)
        {
            SearchForEnemy();
            return;
        }

        lookAtPivot.LookAt(_currentTarget.GetPosition());

        if (_shootingTimer > 0f)
        {
            return;
        }
        
        _shootingTimer = 1f;
        ShootAtEnemy();
    }

    private void SearchForEnemy()
    {
        foreach (var cell in _cellsInRange)
        {
            var enemies = cell.GetEntitiesOfType<SimpleEnemy>();
            foreach (var enemy in enemies)
            {
                if (Vector3.Distance(enemy.GetPosition(), transform.position) <= range)
                {
                    _currentTarget = enemy;
                    return;
                }
            }
        }
    }

    private void ShootAtEnemy()
    {
        var bullet = Instantiate(bulletPrefab).GetComponent<SimpleBullet>();
        bullet.transform.position = shootingPoint.position;
        bullet.currentTarget = _currentTarget;
        bullet.damages = damages;
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
                SpatialAxis.XZ => new Vector3(cell.Coordinates.x + halfCellSize, 0f, cell.Coordinates.y + halfCellSize),
                SpatialAxis.XY => new Vector3(cell.Coordinates.x + halfCellSize, cell.Coordinates.y + halfCellSize, 0f),
                SpatialAxis.YZ => new Vector3(0f, cell.Coordinates.x + halfCellSize, cell.Coordinates.y + halfCellSize),
                _ => throw new ArgumentOutOfRangeException()
            };
            Gizmos.DrawSphere(center, cell.Size / 3f);
        }
    }
}