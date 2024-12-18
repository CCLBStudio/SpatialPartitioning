using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CCLBStudio.SpatialPartitioning.Grid
{
    [Serializable]
    public class SpatialGridCell
    {
#if UNITY_EDITOR
        [SerializeField] private List<string> debugEntitiesDisplay = new();
        private Dictionary<ISpatialGridEntity, string> _entityNamesHash = new();
#endif

        public List<ISpatialGridEntity> Entities => GetEntities();
        public int EntityCount => GetEntities().Count;
        public Vector2Int Coordinates { get; private set; }
        public Vector3 Center { get; private set; }
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }
        public int Size { get; private set; }
        public SpatialGrid Grid { get; private set; }
    
        private List<ISpatialGridEntity> _entities = new();

        public SpatialGridCell(Vector2Int coordinates, SpatialGrid grid)
        {
            Coordinates = coordinates;
            Size = grid.CellSize;
            Grid = grid;
            float halfSize = Size / 2f;

            switch (grid.Axis)
            {
                case SpatialAxis.XZ:
                    Min = new Vector3(coordinates.x, 0f, coordinates.y);
                    Max = new Vector3(coordinates.x + Size, 0f, coordinates.y + Size);
                    Center = new Vector3(coordinates.x + halfSize, 0f, coordinates.y + halfSize);
                    break;
            
                case SpatialAxis.XY:
                    Min = new Vector3(coordinates.x, coordinates.y, 0f);
                    Max = new Vector3(coordinates.x + Size, coordinates.y + Size, 0f);
                    Center = new Vector3(coordinates.x + halfSize, coordinates.y + halfSize, 0f);
                    break;
            
                case SpatialAxis.YZ:
                    Min = new Vector3(0f, coordinates.x, coordinates.y);
                    Max = new Vector3(0f, coordinates.x + Size, coordinates.y + Size);
                    Center = new Vector3(0f, coordinates.x + halfSize, coordinates.y + halfSize);
                    break;
            
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerable<T> GetEntitiesOfType<T>() where T : ISpatialGridEntity
        {
            CleanNullEntities();
            return _entities.OfType<T>();
        }
    
        private List<ISpatialGridEntity> GetEntities()
        {
            CleanNullEntities();
            return _entities;
        }

        public void AddEntity(ISpatialGridEntity gridEntity)
        {
            if (gridEntity.CurrentGridCell == this)
            {
                return;
            }

            gridEntity.CurrentGridCell?.RemoveEntity(gridEntity);
            _entities.Add(gridEntity);
            gridEntity.CurrentGridCell = this;
        
#if UNITY_EDITOR
            if (!Grid.requireDebugDataDisplay)
            {
                return;
            }
            _entityNamesHash.Add(gridEntity, gridEntity.GetDebugName());
            debugEntitiesDisplay = new List<string>(_entityNamesHash.Values);
#endif
        }
    
        public void RemoveEntity(ISpatialGridEntity gridEntity)
        {
            if (gridEntity.CurrentGridCell != this)
            {
                return;
            }
        
            _entities.Remove(gridEntity);
            gridEntity.CurrentGridCell = null;
        
#if UNITY_EDITOR
            if (!Grid.requireDebugDataDisplay)
            {
                return;
            }
            _entityNamesHash.Remove(gridEntity);
            debugEntitiesDisplay = new List<string>(_entityNamesHash.Values);
#endif
        }

        public void CleanNullEntities()
        {
            if (!Grid.AutomaticCheckForNullEntities)
            {
                return;
            }
        
            List<ISpatialGridEntity> result = new List<ISpatialGridEntity>();
        
            foreach (var e in _entities)
            {
                switch (e)
                {
                    case MonoBehaviour me:
                    {
                        if (me)
                        {
                            result.Add(e);
                        }

                        continue;
                    }
                
                    case ScriptableObject se:
                    {
                        if (se)
                        {
                            result.Add(e);
                        }

                        continue;
                    }
                }

                if (e != null)
                {
                    result.Add(e);
                }
            }

            result.TrimExcess();
            _entities = result;
        }
    }
}