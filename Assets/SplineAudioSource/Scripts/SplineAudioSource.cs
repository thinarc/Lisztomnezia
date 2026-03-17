using UnityEngine;
using UnityEditor;
using Vector3 = UnityEngine.Vector3;

namespace SplineAS
{
    public enum SplineType
    {
        Quadratic,
        Line
    }

    public class SplineAudioSource : MonoBehaviour
    {
        
        public SplineType splineType = SplineType.Quadratic;
        public bool useWorldPosition = true;

        [HideInInspector] public Transform target;
        [HideInInspector] public Path path;
        [HideInInspector] public bool closePath;
        [HideInInspector] public bool followTargetInside;

        // EDITOR
        [HideInInspector] public float lineThickness = 2f;
        [HideInInspector] public float handleSize = 1f;
        [HideInInspector] public float snapOffset = 1f;
        [HideInInspector] public Color lineColor = Color.white;
        [HideInInspector] public Color handleColor = Color.white;
        [HideInInspector] public Color controlColor = new Color(1, 0.6f, 0, 1f);
        [HideInInspector] public Color selectedHandleColor = Color.green;

        private const float SMOOTH_TIME = .05f;
        private Vector3 _velocity = Vector3.zero;
        private AudioSource _audioSource;
        
        private void Awake()
        {
            if(path == null)
                this.enabled = false;
            
            if (target == null)
            {
                Debug.LogError("[Spline Audio Source] - Target not assigned.");
                this.enabled = false;
            }

            if (GetComponentInChildren<AudioSource>() != null)
            {
                _audioSource = gameObject.GetComponentInChildren<AudioSource>();
            }
            else
            {
                this.enabled = false;
                Debug.LogError("[Spline Audio Source] - Audio Source component not found on children.");
            }

            _audioSource.transform.position = GetClosestPointOnPath(target.position);
        }

        void Update()
        {
            if (path == null || !target)
                return;
            
            // Optimizar

            switch (splineType)
            {
                case SplineType.Quadratic:
                    Vector3 closestPointQuad = GetClosestPointOnPath(target.position);
                    _audioSource.transform.position =
                        Vector3.SmoothDamp(_audioSource.transform.position, closestPointQuad, ref _velocity, SMOOTH_TIME);
                    break;

                case SplineType.Line:
                    Vector3 closestPointLine = GetClosestPointOnPath(target.position);
                    _audioSource.transform.position =
                        Vector3.SmoothDamp(_audioSource.transform.position, closestPointLine, ref _velocity, SMOOTH_TIME);
                    break;

                default:
                    splineType = SplineType.Quadratic;
                    break;
            }
        }
        
        Vector3 GetClosestPointOnPath(Vector3 targetPosition)
        {
            if (followTargetInside && closePath && path.IsPointInsideCurveXZ(targetPosition))
                return targetPosition;

            Vector3[] pathPoints = path.GetPathPoints();
            int pathLength = pathPoints.Length;

            if(!useWorldPosition)
                for (int i = 0; i < pathLength; i++)
                {
                    Vector3 scale = transform.localScale;

                    pathPoints[i] = (transform.right * (pathPoints[i].x * scale.x)) +
                                    (transform.up * (pathPoints[i].y * scale.y)) +
                                    (transform.forward * (pathPoints[i].z * scale.z));

                    pathPoints[i] += transform.position;
                }

            if (pathLength < 2) 
                return _audioSource.transform.position;

            float minDistance = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;

            void EvaluateSegment(Vector3 p0, Vector3 p1, Vector3 p2)
            {
                int stepResolution = Mathf.Clamp((int)(Vector3.Distance(p0, p2) * 0.5f), 1, 30);
                float resolution = 1f / stepResolution;

                for (float t = 0; t <= 1.0f; t += resolution)
                {
                    Vector3 pointOnCurve = GetPointOnQuadraticBezier(p0, p1, p2, t);
                    float distance = Vector3.SqrMagnitude(targetPosition - pointOnCurve); // Usa SqrMagnitude en lugar de Distance

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPoint = pointOnCurve;
                    }
                }
            }

            for (int i = 0; i < pathLength - 2; i++)
                EvaluateSegment(pathPoints[i], pathPoints[i + 1], pathPoints[i + 2]);

            if (closePath)
                EvaluateSegment(pathPoints[pathLength - 1], pathPoints[0], pathPoints[1]);

            return closestPoint;
        }

        Vector3 GetPointOnQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            return uu * p0 + 2 * u * t * p1 + tt * p2;
        }

        public void CreatePath(Vector3 position)
        {
            path = new Path(position);
        }

        #region Copy Paste Path
#if UNITY_EDITOR
        
        [ContextMenu("Copy Path to Clipboard")]
        public void CopyPathToClipboard()
        {
            Vector3ArrayWrapper wrapper = new Vector3ArrayWrapper(path.GetPathPoints());
            string pathAsJson = JsonUtility.ToJson(wrapper);

            GUIUtility.systemCopyBuffer = pathAsJson;
            Debug.Log("Path copied to clipboard");
        }

        [ContextMenu("Paste Path From Clipboard")]
        public void PastePathFromClipboard()
        {
            Undo.RecordObject(this, "Paste Path From Clipboard");

            string clipboardData = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clipboardData))
            {
                Debug.LogWarning("Clipboard is empty or invalid.");
                return;
            }

            try
            {
                Vector3ArrayWrapper wrapper = JsonUtility.FromJson<Vector3ArrayWrapper>(clipboardData);

                if (wrapper != null && wrapper.Array != null)
                {
                    path.SetPathPoints(wrapper.Array);
                    Debug.Log("Path pasted from clipboard.");
                }
                else
                {
                    Debug.LogWarning("Clipboard data is invalid or empty.");
                }

                EditorUtility.SetDirty(this);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to paste path from clipboard: " + e.Message);
            }
        }

        [System.Serializable]
        private class Vector3ArrayWrapper
        {
            public Vector3[] Array;

            public Vector3ArrayWrapper(Vector3[] array)
            {
                Array = array;
            }
        }
        
#endif
        #endregion
    }

#if UNITY_EDITOR
    public static class PathCreatorMenu
    {
        [MenuItem("GameObject/Audio/Spline Audio Source", false, 10)]
        public static void CreatePathCreator(MenuCommand menuCommand)
        {
            GameObject gameObject = new GameObject("Spline Audio Source");
            GameObject childGameObject = new GameObject("Audio Source (Spline Audio Source)");
            childGameObject.transform.parent = gameObject.transform;
            
            gameObject.AddComponent<SplineAudioSource>();
            
            childGameObject.AddComponent<AudioSource>();
            childGameObject.GetComponent<AudioSource>().loop = true;
            childGameObject.GetComponent<AudioSource>().spatialBlend = 1f;
            childGameObject.GetComponent<AudioSource>().minDistance = 5f;
            childGameObject.GetComponent<AudioSource>().maxDistance = 100f;
            childGameObject.GetComponent<AudioSource>().dopplerLevel = .5f;

            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);

            Selection.activeObject = gameObject;

            Undo.RegisterCreatedObjectUndo(gameObject, "Create Spline Audio Source");
        }
    }
#endif
}