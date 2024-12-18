using UnityEngine;

namespace CCLBStudio.SpatialPartitioning.QuadTree
{
    public struct QuadTreeRect
    {
        public Vector2 Center;
        public Vector2 Size;
        
        public QuadTreeRect(Vector2 center, Vector2 size)
        {
            Center = center;
            Size = size;
        }

        public bool Contains(Vector2 point)
        {
            return point.x >= Center.x - Size.x / 2 &&
                   point.x <= Center.x + Size.x / 2 &&
                   point.y >= Center.y - Size.y / 2 &&
                   point.y <= Center.y + Size.y / 2;
        }

        public bool Intersects(QuadTreeRect other)
        {
            return !(other.Center.x - other.Size.x / 2 > Center.x + Size.x / 2 ||
                     other.Center.x + other.Size.x / 2 < Center.x - Size.x / 2 ||
                     other.Center.y - other.Size.y / 2 > Center.y + Size.y / 2 ||
                     other.Center.y + other.Size.y / 2 < Center.y - Size.y / 2);
        }
    }
}