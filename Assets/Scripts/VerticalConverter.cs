using UnityEngine;

namespace CandyCrush
{
    public abstract class CoordinateConverter
    {
        public abstract Vector3 GridToWorld(int x, int y, float cellSize, Vector3 origin);

        public abstract Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 origin);

        public abstract Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 origin);

        public abstract Vector3 Forward { get; }
    }

    /// <summary>
    /// A coordinate converter for vertical grids, where the grid lies on the X-Y plane.
    /// </summary>
    public class VerticalConverter : CoordinateConverter
    {
        public override Vector3 GridToWorld(int x, int y, float cellSize, Vector3 origin)
        {
            return new Vector3(x, y, 0) * cellSize + origin;
        }

        public override Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 origin)
        {
            return new Vector3(x * cellSize + cellSize * 0.5f, y * cellSize + cellSize * 0.5f, 0) + origin;
        }

        public override Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 origin)
        {
            Vector3 gridPosition = (worldPosition - origin) / cellSize;
            var x = Mathf.FloorToInt(gridPosition.x);
            var y = Mathf.FloorToInt(gridPosition.y);
            return new Vector2Int(x, y);
        }

        public override Vector3 Forward => Vector3.forward;
    }
}