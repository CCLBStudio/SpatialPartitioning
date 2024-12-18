using UnityEngine;

namespace CCLBStudio.SpatialPartitioning.QuadTree
{
    [CreateAssetMenu(menuName = "CCLB Studio/Spatial Partitioning/Spatial Quad Tree")]
    public class SpatialQuadTree : ScriptableObject
    {
        [Tooltip("The quad tree axis, determining how the spatial checks will be performed.")]
        [SerializeField] private SpatialAxis axis = SpatialAxis.XZ;
        [Tooltip("Maximum entities inside a quad before performing a subdivision.")]
        [Min(2)][SerializeField] private int maxCapacity = 4;
        [Tooltip("If TRUE, the quad tree will initialize itself during the OnEnable method.")]
        [SerializeField] private bool autoInitialize = true;
    }
}