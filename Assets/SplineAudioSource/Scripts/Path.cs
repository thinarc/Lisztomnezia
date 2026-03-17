using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SplineAS
{
    [System.Serializable]
    public class Path
    {
        [SerializeField]
        List<Vector3> pathPositions = new List<Vector3>();

        public Path(Vector3 center)
        {
            pathPositions.Add(center + (Vector3.up * 10f));
            pathPositions.Add(center + (Vector3.up * 10f) + (Vector3.right * 5f) + (Vector3.forward * 5f));
            pathPositions.Add(center + (Vector3.up * 10f) + (Vector3.right * 10f));
        }

        public Vector3 GetPointByIndex(int i)
        {
            return pathPositions[i];
        }

        public Vector3[] GetPathPoints()
        {
            if (pathPositions != null)
                return pathPositions.ToArray();
            else
                return null;
        }

        public void SetPathPoints(Vector3[] newPathPositions)
        {
            pathPositions = newPathPositions.ToList();
        }

        public void SetPathPoint(int i, Vector3 newPathPosition)
        {
            pathPositions[i] = newPathPosition;
        }

        public void AddNewPoint(Vector3 pos)
        {
            pathPositions.Add(pos);
        }

        public void AddNewPointAtIndex(int index, Vector3 pos)
        {
            if (index < 0 || index > pathPositions.Count)
            {
                Debug.LogWarning("[Spline Audio Source] - Index out of range");
                return;
            }

            pathPositions.Insert(index, pos);
        }

        public void RemoveLastPoint()
        {
            if (pathPositions.Count > 0)
                pathPositions.RemoveAt(pathPositions.Count - 1);
        }

        public void RemovePointAtIndex(int index)
        {
            if (index >= 0 && index < pathPositions.Count)
                pathPositions.RemoveAt(index);
        }

        public bool IsPointInsideCurveXZ(Vector3 point)
        {
            List<Vector2> projectedCurve = ProjectCurveToXZ(pathPositions);
            Vector2 projectedPoint = new Vector2(point.x, point.z);
            return IsPointInsidePolygon(projectedCurve, projectedPoint);
        }

        private List<Vector2> ProjectCurveToXZ(List<Vector3> curve)
        {
            List<Vector2> projectedCurve = new List<Vector2>();
            foreach (var p in curve)
            {
                projectedCurve.Add(new Vector2(p.x, p.z));
            }
            return projectedCurve;
        }

        private bool IsPointInsidePolygon(List<Vector2> polygon, Vector2 point)
        {
            int n = polygon.Count;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                // Usamos la componente Z en lugar de Y, ya que estamos trabajando en el plano XZ
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
